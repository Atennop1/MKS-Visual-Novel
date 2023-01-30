// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class AboutWindow : EditorWindow
    {
        public static string InstalledVersion { get => PlayerPrefs.GetString(installedVersionKey); set => PlayerPrefs.SetString(installedVersionKey, value); }

        private const string installedVersionKey = "Naninovel." + nameof(AboutWindow) + "." + nameof(InstalledVersion);
        private const string releaseNotesUri = "https://github.com/Naninovel/Documentation/releases";
        private const string guideUri = "https://naninovel.com/guide/getting-started.html";
        private const string commandsUri = "https://naninovel.com/api/";
        private const string forumUri = "https://forum.naninovel.com";
        private const string discordUri = "https://discord.gg/BfkNqem";
        private const string supportUri = "https://naninovel.com/support/";
        private const string reviewUri = "https://assetstore.unity.com/packages/templates/systems/naninovel-visual-novel-engine-135453#reviews";

        private const int windowWidth = 328;
        private const int windowHeight = 445;

        private EngineVersion engineVersion;
        private GUIContent logoContent;

        private void OnEnable ()
        {
            engineVersion = EngineVersion.LoadFromResources();
            InstalledVersion = engineVersion.Version;
            var logoPath = PathUtils.AbsoluteToAssetPath(Path.Combine(PackagePath.EditorResourcesPath, "NaninovelLogo.png"));
            logoContent = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath));
        }

        public void OnGUI ()
        {
            var rect = new Rect(5, 10, windowWidth - 10, windowHeight);
            GUILayout.BeginArea(rect);

            GUILayout.BeginHorizontal();
            GUILayout.Space(57);
            GUILayout.BeginVertical();

            GUILayout.Label(logoContent, GUIStyle.none);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(93);
            EditorGUILayout.SelectableLabel($"{engineVersion.Version} build {engineVersion.Build}");
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Release Notes", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("For the list of changes associated with this release, see the notes on GitHub.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Release Notes")) Application.OpenURL(releaseNotesUri + $"/tag/{engineVersion.Version}");

            GUILayout.Space(7);

            EditorGUILayout.LabelField("Online Resources", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Please read getting started and command guides. Contact support if you have any issues or questions.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal();
           
            if (GUILayout.Button("Guide")) Application.OpenURL(guideUri);
            if (GUILayout.Button("Commands")) Application.OpenURL(commandsUri);
            if (GUILayout.Button("Forum")) Application.OpenURL(forumUri);
            if (GUILayout.Button("Discord")) Application.OpenURL(discordUri);
            if (GUILayout.Button("Support")) Application.OpenURL(supportUri);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(7);

            EditorGUILayout.LabelField("Rate Naninovel", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("We really hope you like Naninovel! If you feel like it, please leave a review on the Asset Store.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Review on Asset Store")) Application.OpenURL(reviewUri);

            GUILayout.EndArea();
        }

        [InitializeOnLoadMethod]
        private static void FirstTimeSetup ()
        {
            EditorApplication.delayCall += ExecuteFirstTimeSetup;
        }

        private static void ExecuteFirstTimeSetup ()
        {
            // First time ever launch.
            if (string.IsNullOrWhiteSpace(InstalledVersion))
            {
                OpenWindow();
                return;
            }

            // First time after update launch.
            var engineVersion = EngineVersion.LoadFromResources();
            if (engineVersion && engineVersion.Version != InstalledVersion)
                OpenWindow();
        }

        [MenuItem("Naninovel/About", priority = 0)]
        private static void OpenWindow ()
        {
            var position = new Rect(100, 100, windowWidth, windowHeight);
            GetWindowWithRect<AboutWindow>(position, true, "About Naninovel", true);
        }
    }
}
