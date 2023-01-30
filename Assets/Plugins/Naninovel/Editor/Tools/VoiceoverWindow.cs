// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class VoiceoverWindow : EditorWindow
    {
        protected string OutputPath { get => PlayerPrefs.GetString(outputPathKey); set => PlayerPrefs.SetString(outputPathKey, value); }
        protected bool UseMarkdownFormat { get => PlayerPrefs.GetInt(useMarkdownFormatKey) == 1; set => PlayerPrefs.SetInt(useMarkdownFormatKey, value ? 1 : 0); }

        private static readonly GUIContent localeLabel = new GUIContent("Locale");
        private static readonly GUIContent useMdLabel = new GUIContent("Use Markdown Format", "Whether to produce markdown (.md) instead of plain text (.txt) files with some formatting for better readability.");

        private const string outputPathKey = "Naninovel." + nameof(VoiceoverWindow) + "." + nameof(OutputPath);
        private const string useMarkdownFormatKey = "Naninovel." + nameof(VoiceoverWindow) + "." + nameof(UseMarkdownFormat);

        private bool isWorking = false;
        private IScriptManager scriptsManager;
        private ILocalizationManager localizationManager;
        private string locale = null;

        [MenuItem("Naninovel/Tools/Voiceover Documents")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 160);
            GetWindowWithRect<VoiceoverWindow>(position, true, "Voiceover Documents", true);
        }

        private void OnEnable ()
        {
            if (!Engine.Initialized)
            {
                isWorking = true;
                Engine.OnInitializationFinished += InitializeEditor;
                EditorInitializer.InitializeAsync().Forget();
            }
            else InitializeEditor();
        }

        private void OnDisable ()
        {
            Engine.Destroy();
        }

        private void InitializeEditor ()
        {
            Engine.OnInitializationFinished -= InitializeEditor;

            scriptsManager = Engine.GetService<IScriptManager>();
            localizationManager = Engine.GetService<ILocalizationManager>();
            locale = ProjectConfigurationProvider.LoadOrDefault<LocalizationConfiguration>().SourceLocale;
            isWorking = false;
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Voiceover Documents", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate voiceover documents; see `Voicing` guide for usage instructions.", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (isWorking)
            {
                EditorGUILayout.HelpBox("Working, please wait...", MessageType.Info);
                return;
            }

            locale = LocalesPopupDrawer.Draw(locale, localeLabel);
            UseMarkdownFormat = EditorGUILayout.Toggle(useMdLabel, UseMarkdownFormat);
            using (new EditorGUILayout.HorizontalScope())
            {
                OutputPath = EditorGUILayout.TextField("Output Path", OutputPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    OutputPath = EditorUtility.OpenFolderPanel("Output Path", "", "");
            }

            GUILayout.FlexibleSpace();

            if (!localizationManager.LocaleAvailable(locale))
                EditorGUILayout.HelpBox($"Selected locale is not available. Make sure a `{locale}` directory exists in the localization resources.", MessageType.Warning, true);
            else
            {
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(OutputPath));
                if (GUILayout.Button("Generate Voiceover Documents", GUIStyles.NavigationButton))
                    GenerateVoiceoverDocumentsAsync();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space();
        }

        private async void GenerateVoiceoverDocumentsAsync ()
        {
            try
            {
                isWorking = true;

                EditorUtility.DisplayProgressBar("Generating Voiceover Documents", "Initializing...", 0f);

                await localizationManager.SelectLocaleAsync(locale);

                var scripts = await scriptsManager.LoadAllScriptsAsync();
                WriteVoiceoverDocuments(scripts.ToList());

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                isWorking = false;
                Repaint();

                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate voiceover documents: {e.Message}");
                isWorking = false;
                EditorUtility.ClearProgressBar(); 
            }
        }

        private void WriteVoiceoverDocuments (List<Script> scripts)
        {
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            new DirectoryInfo(OutputPath).GetFiles().ToList().ForEach(f => f.Delete());

            for (int i = 0; i < scripts.Count; i++)
            {
                var script = scripts[i];
                var progress = i / (float)scripts.Count;
                EditorUtility.DisplayProgressBar("Generating Voiceover Documents", $"Processing `{script.name}` script...", progress);

                var scriptText = $"# Voiceover document for script '{script.Name}' ({locale ?? "default"} locale)\n\n";
                var commands = new ScriptPlaylist(script, scriptsManager);
                foreach (var cmd in commands)
                {
                    if (!(cmd is PrintText)) continue;
                    var printCmd = cmd as PrintText;

                    var autoVoicePath = AudioConfiguration.GetAutoVoiceClipPath(printCmd.PlaybackSpot);
                    scriptText += UseMarkdownFormat ? $"## {autoVoicePath}\n" : $"{autoVoicePath}\n";
                    if (!string.IsNullOrEmpty(printCmd.AuthorId))
                        scriptText += $"{printCmd.AuthorId}: ";
                    scriptText += UseMarkdownFormat ? $"`{printCmd.Text}`\n\n" : $"{printCmd.Text}\n\n";
                }

                var fileExtension = UseMarkdownFormat ? "md" : "txt";
                File.WriteAllText($"{OutputPath}/{script.Name}.{fileExtension}", scriptText);
            }
        }
    }
}
