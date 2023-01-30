// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    [CustomEditor(typeof(EngineVersion))]
    public class EngineVersionEditor : Editor
    {
        protected static string GitHubProjectPath => PlayerPrefs.GetString(nameof(GitHubProjectPath), string.Empty);

        private const string packageTextTemplate = @"{
    ""name"": ""com.elringus.naninovel"",
    ""version"": ""{VERSION}"",
    ""displayName"": ""Naninovel"",
    ""description"": ""A full-featured, writer-friendly and completely customizable visual novel extension for Unity game engine"",
    ""unity"": ""2019.4"",
    ""author"": {
        ""name"": ""Elringus"",
        ""url"": ""https://naninovel.com""
    }
}";

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Update", GUIStyles.NavigationButton))
                Update();
        }

        public static void Update ()
        {
            var asset = EngineVersion.LoadFromResources();
            using (var serializedObj = new SerializedObject(asset))
            {
                // 1. Update engine version asset.
                serializedObj.Update();

                var engineVersionProperty = serializedObj.FindProperty("engineVersion");
                var buildDateProperty = serializedObj.FindProperty("buildDate");

                // Try resolve git version option #1.
                var gitPath = $"{GitHubProjectPath}/.git/refs/tags";
                if (Directory.Exists(gitPath))
                    engineVersionProperty.stringValue = Directory.GetFiles(gitPath)
                        .Where(p => !p.EndsWith("beta")).Select(p => p.GetAfter("v1.")).Max();
            
                // Try resolve git version option #2.
                if (string.IsNullOrWhiteSpace(engineVersionProperty.stringValue))
                {
                    gitPath = $"{GitHubProjectPath}/.git/packed-refs";
                    if (File.Exists(gitPath))
                        engineVersionProperty.stringValue = File.ReadAllText(gitPath).SplitByNewLine()
                            ?.Where(l => l.Contains("refs/tags/v") && !l.EndsWith("beta")).Select(l => l.GetAfter("v1.").TrimFull()).Max();
                }

                engineVersionProperty.stringValue = "v1." + engineVersionProperty.stringValue;

                buildDateProperty.stringValue = $"{DateTime.Now:yyyy-MM-dd}";

                serializedObj.ApplyModifiedProperties();

                // 2. Update package version.
                var packageVersion = engineVersionProperty.stringValue.GetAfter("v");
                var packageText = packageTextTemplate.Replace("{VERSION}", packageVersion);
                var packagePath = PathUtils.Combine(PackagePath.PackageRootPath, "package.json");
                File.WriteAllText(packagePath, packageText);

                // 3. Serialize the files.
                EditorUtility.SetDirty(asset);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        private static DateTime ParseBuildDate (string buildDate)
        {
            var parsed = DateTime.TryParseExact(buildDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
            return parsed ? result : DateTime.MinValue;
        }
    }
}
