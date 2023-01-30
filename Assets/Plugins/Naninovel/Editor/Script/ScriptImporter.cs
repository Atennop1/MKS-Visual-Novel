// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.IO;
using System.Text;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace Naninovel
{
    [ScriptedImporter(version: 34, ext: "nani")]
    public class ScriptImporter : ScriptedImporter
    {
        public override void OnImportAsset (AssetImportContext ctx)
        {
            var contents = string.Empty;

            try
            {
                var bytes = File.ReadAllBytes(ctx.assetPath);
                contents = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                // Purge BOM. Unity auto adding it when creating script assets: https://git.io/fjVgY
                if (contents.Length > 0 && contents[0] == '\uFEFF')
                {
                    contents = contents.Substring(1);
                    File.WriteAllText(ctx.assetPath, contents);
                }
            }
            catch (IOException exc)
            {
                ctx.LogImportError($"IOException: {exc.Message}");
            }
            finally
            {
                var assetName = Path.GetFileNameWithoutExtension(ctx.assetPath);
                var asset = Script.FromScriptText(assetName, contents);
                asset.hideFlags = HideFlags.NotEditable;

                ctx.AddObjectToAsset("naniscript", asset);
                ctx.SetMainObject(asset);
            }
        }
    }
}
