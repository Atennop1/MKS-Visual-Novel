// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class ScriptAssetPostprocessor : AssetPostprocessor
    {
        private static ScriptsConfiguration configuration = default;
        private static EditorResources editorResources = default;

        private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (BuildProcessor.Building) return;
            // Delayed call is required to prevent running when re-importing all assets,
            // at which point editor resources asset is not available.
            EditorApplication.delayCall += () => PostprocessDelayed(importedAssets);
        }

        private static void PostprocessDelayed (string[] importedAssets)
        {
            if (configuration is null)
                configuration = ProjectConfigurationProvider.LoadOrDefault<ScriptsConfiguration>();
            if (editorResources is null)
                editorResources = EditorResources.LoadOrDefault();

            var modifiedResource = false;
            var importedDirs = new HashSet<string>();

            foreach (string assetPath in importedAssets)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(Script)) continue;
                HandleAutoAdd(assetPath, ref modifiedResource);
                importedDirs.Add(Path.GetDirectoryName(Path.GetFullPath(assetPath)));
            }

            if (modifiedResource)
            {
                EditorUtility.SetDirty(editorResources);
                AssetDatabase.SaveAssets();
            }

            if (importedDirs.Count > 0)
                ScriptFileWatcher.AddWatchedDirectories(importedDirs);
        }

        private static void HandleAutoAdd (string assetPath, ref bool modifiedResource)
        {
            if (!configuration.AutoAddScripts) return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var name = Path.GetFileNameWithoutExtension(assetPath);

            // Don't add the script if it's already added.
            if (guid is null || editorResources.GetPathByGuid(guid) != null) return;

            // Add only new scripts created via context menu (will always have a @stop at second line).
            var linesEnum = File.ReadLines(assetPath).GetEnumerator();
            var secondLine = (linesEnum.MoveNext() && linesEnum.MoveNext()) ? linesEnum.Current : null;
            linesEnum.Dispose(); // Release the file.
            if (!secondLine?.EqualsFast(AssetMenuItems.DefaultScriptContent.GetAfterFirst(Environment.NewLine)) ?? true) return;
            
            // Don't add if another with the same name is already added.
            if (editorResources.Exists(name, configuration.Loader.PathPrefix, configuration.Loader.PathPrefix))
            {
                Debug.LogError($"Failed to add `{name}` script: another script with the same name is already added. " +
                               $"Either delete the existing script or use another name.");
                AssetDatabase.MoveAssetToTrash(assetPath);
                return;
            }

            editorResources.AddRecord(configuration.Loader.PathPrefix, configuration.Loader.PathPrefix, name, guid);
            modifiedResource = true;
        }
    }

    public class ScriptAssetProcessor : UnityEditor.AssetModificationProcessor
    {
        private static EditorResources editorResources = default;

        private static AssetDeleteResult OnWillDeleteAsset (string assetPath, RemoveAssetOptions options)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(Script))
                return AssetDeleteResult.DidNotDelete;

            if (editorResources is null)
                editorResources = EditorResources.LoadOrDefault();

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (guid is null) return AssetDeleteResult.DidNotDelete;

            editorResources.RemoveAllRecordsWithGuid(guid);
            EditorUtility.SetDirty(editorResources);
            AssetDatabase.SaveAssets();

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
