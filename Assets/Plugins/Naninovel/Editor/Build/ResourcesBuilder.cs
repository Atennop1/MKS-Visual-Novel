// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Naninovel
{
    public class ResourcesBuilder
    {
        private const string tempResourcesPath = "Assets/TEMP_NANINOVEL/Resources";
        private const string tempStreamingPath = "Assets/StreamingAssets";

        private readonly ResourceProviderConfiguration config;
        private readonly IAddressableBuilder addressableBuilder;
        private readonly IDictionary<string, Type> projectResources = new Dictionary<string, Type>();

        public ResourcesBuilder (ResourceProviderConfiguration config, IAddressableBuilder addressableBuilder)
        {
            this.config = config;
            this.addressableBuilder = addressableBuilder;
        }

        public static void Cleanup ()
        {
            AssetDatabase.DeleteAsset(tempResourcesPath.GetBeforeLast("/"));
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        public void Build (BuildPlayerOptions options)
        {
            PrepareForBuild();

            var records = EditorResources.LoadOrDefault().GetAllRecords();
            var progress = 0f;
            foreach (var record in records)
            {
                DisplayProgressBar($"Processing '{record.Key}'", progress++ / records.Count);
                ProcessRecord(record.Value, record.Key, options.target == BuildTarget.WebGL);
            }

            AssetDatabase.SaveAssets();

            if (config.AutoBuildBundles)
                addressableBuilder.BuildContent();
        }

        private void PrepareForBuild ()
        {
            EditorUtils.CreateFolderAsset(tempResourcesPath);
            ProjectResourcesBuildProcessor.TempFolderPath = tempResourcesPath;
            addressableBuilder.RemoveEntries();
            UpdateProjectResources();
        }

        private void UpdateProjectResources ()
        {
            projectResources.Clear();
            var filter = GetProjectResourcesFilter();
            foreach (var resource in ProjectResources.Get().GetAllResources(filter))
                projectResources[resource.Key] = resource.Value;
        }

        private string GetProjectResourcesFilter ()
        {
            if (string.IsNullOrEmpty(config.ProjectRootPath)) return null;
            return $"{config.ProjectRootPath}/";
        }

        private void ProcessRecord (string assetGuid, string resourcePath, bool webgl)
        {
            if (!TryGetAssetPath(assetGuid, resourcePath, out var assetPath)) return;
            if (!TryGetAssetType(assetPath, out var assetType)) return;
            if (assetType == typeof(SceneAsset)) ProcessSceneResource(assetPath);
            else if (assetType == typeof(VideoClip) && webgl) ProcessVideoForWebGL(resourcePath, assetPath);
            else ProcessResourceAsset(assetGuid, resourcePath, assetPath);
        }

        private bool TryGetAssetPath (string assetGuid, string resourcePath, out string assetPath)
        {
            assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (!string.IsNullOrEmpty(assetPath)) return true;
            Debug.LogWarning($"Failed to resolve `{resourcePath}` resource GUID. The resource won't be included to the build.");
            return false;
        }

        private bool TryGetAssetType (string assetPath, out Type assetType)
        {
            assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (assetType != null) return true;
            Debug.LogWarning($"Failed to evaluate type of `{assetPath}` asset. The asset won't be included to the build.");
            return false;
        }

        private static void ProcessSceneResource (string assetPath)
        {
            var currentScenes = EditorBuildSettings.scenes.ToList();
            if (string.IsNullOrEmpty(assetPath) || currentScenes.Exists(s => s.path == assetPath)) return;
            currentScenes.Add(new EditorBuildSettingsScene(assetPath, true));
            EditorBuildSettings.scenes = currentScenes.ToArray();
        }

        private static void ProcessVideoForWebGL (string resourcePath, string assetPath)
        {
            var streamingPath = PathUtils.Combine(tempStreamingPath, resourcePath);
            if (assetPath.Contains(".")) streamingPath += $".{assetPath.GetAfter(".")}";
            if (assetPath.EndsWithFast(streamingPath)) return; // Already in the streaming folder.
            EditorUtils.CreateFolderAsset(streamingPath.GetBeforeLast("/"));
            AssetDatabase.CopyAsset(assetPath, streamingPath);
        }

        private void ProcessResourceAsset (string assetGuid, string resourcePath, string assetPath)
        {
            if (IsProjectResource(assetGuid, resourcePath)) return;
            if (addressableBuilder.TryAddEntry(assetGuid, resourcePath)) return;
            CopyTemporaryAsset(assetPath, resourcePath);
        }

        private bool IsProjectResource (string assetGuid, string resourcePath)
        {
            if (!projectResources.ContainsKey(resourcePath)) return false;
            CheckProjectResourceConflict(assetGuid, resourcePath);
            return true;
        }

        private void CheckProjectResourceConflict (string assetGuid, string resourcePath)
        {
            var otherResourcePath = (GetProjectResourcesFilter() ?? string.Empty) + resourcePath;
            var otherAsset = Resources.Load(otherResourcePath);
            if (!otherAsset) return;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(otherAsset, out var otherGuid, out long _);
            if (otherGuid == assetGuid) return;
            var otherPath = AssetDatabase.GetAssetPath(otherAsset);
            throw new InvalidOperationException($"Resource conflict detected: asset stored at `{otherPath}` conflicts with " +
                                                $"`{resourcePath}` Naninovel resource; rename or move the conflicting asset.");
        }

        private void CopyTemporaryAsset (string assetPath, string resourcePath)
        {
            var tempPath = string.IsNullOrWhiteSpace(config.ProjectRootPath)
                ? PathUtils.Combine(tempResourcesPath, resourcePath)
                : PathUtils.Combine(tempResourcesPath, config.ProjectRootPath, resourcePath);
            if (assetPath.Contains(".")) tempPath += $".{assetPath.GetAfter(".")}";
            EditorUtils.CreateFolderAsset(tempPath.GetBeforeLast("/"));
            AssetDatabase.CopyAsset(assetPath, tempPath);
        }

        private static void DisplayProgressBar (string job, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing Naninovel Resources", $"{job}...", progress))
                throw new OperationCanceledException("Build was cancelled by the user.");
        }
    }
}
