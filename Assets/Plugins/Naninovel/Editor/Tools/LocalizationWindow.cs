// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Naninovel.Parsing;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class LocalizationWindow : EditorWindow
    {
        protected string SourceScriptsPath
        {
            get => PlayerPrefs.GetString(sourceScriptsPathKey);
            set
            {
                PlayerPrefs.SetString(sourceScriptsPathKey, value);
                ValidateOutputPath();
            }
        }
        protected string SourceManagedTextPath
        {
            get => PlayerPrefs.GetString(sourceManagedTextPathKey, $"{Application.dataPath}/Resources/Naninovel/{ProjectConfigurationProvider.LoadOrDefault<ManagedTextConfiguration>().Loader.PathPrefix}");
            set => PlayerPrefs.SetString(sourceManagedTextPathKey, value);
        }
        protected string LocaleFolderPath
        {
            get => PlayerPrefs.GetString(localeFolderPathKey);
            set
            {
                PlayerPrefs.SetString(localeFolderPathKey, value);
                ValidateOutputPath();
            }
        }

        private const string sourceScriptsPathKey = "Naninovel." + nameof(LocalizationWindow) + "." + nameof(SourceScriptsPath);
        private const string sourceManagedTextPathKey = "Naninovel." + nameof(LocalizationWindow) + "." + nameof(SourceManagedTextPath);
        private const string localeFolderPathKey = "Naninovel." + nameof(LocalizationWindow) + "." + nameof(LocaleFolderPath);
        private const string progressBarTitle = "Generating Localization Resources";

        private static readonly GUIContent localeFolderPathContent = new GUIContent("Locale Folder (output)", "The folder for the target locale where to store generated localization resources. Should be inside localization root (`Assets/Resources/Naninovel/Localization` by default) and have a name equal to one of the supported localization tags.");
        private static readonly GUIContent sourceScriptsPathContent = new GUIContent("Script Folder (input)", "When points to a folder with a previously generated script localization documents, will extract the source text to translate from them instead of the original (source locale) scripts.");
        private static readonly GUIContent sourceManagedTextPathContent = new GUIContent("Text Folder (input)", "Folder under which the source managed text documents are stored (`Resources/Naninovel/Text` by default).");
        private static readonly GUIContent localizeManagedTextContent = new GUIContent("Localize Managed Text", "Whether to also generate localization documents for the managed text.");
        private static readonly GUIContent tryUpdateContent = new GUIContent("Try Update", "Whether to preserve existing translation for the lines that didn't change.");
        private static readonly GUIContent autoTranslateContent = new GUIContent("Auto Translate", "Whether to provide Google Translate machine translation for the missing lines. Command lines and injected expressions won't be affected.\n\nBe aware, that public Google Translate web API limits request frequency per IP and won't process too much text at a time; the service could also sometimes fail to translate particular text causing warnings during the process.");
        private static readonly GUIContent warnUntranslatedContent = new GUIContent("Warn Untranslated", "Whether to log warnings when untranslated lines are found while generating documents with `Try Update` enabled.");

        private static readonly Regex CaptureInlinedRegex = new Regex(@"(?<!\\)\[(.*)(?<!\\)\]", RegexOptions.Compiled); // The same as DynamicValueData.CaptureExprRegex, but for square brackets.
        private static readonly Regex CaptureTagsRegex = new Regex(@"(?<!\\)\<(.*)(?<!\\)\>", RegexOptions.Compiled); // The same as DynamicValueData.CaptureExprRegex, but for angle brackets.

        private readonly List<string> availableLocalizations = new List<string>();
        private bool localizationRootSelected => availableLocalizations.Count > 0;
        private LocalizationConfiguration config;
        private bool tryUpdate = true, localizeManagedText = true, warnUntranslated = false;
        private int wordCount = -1;
        private bool outputPathValid = false, scriptSourcePathValid = false;
        private string targetTag, targetLanguage, sourceTag, sourceLanguage;

        [MenuItem("Naninovel/Tools/Localization")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 325);
            GetWindowWithRect<LocalizationWindow>(position, true, "Localization", true);
        }

        private void OnEnable ()
        {
            config = ProjectConfigurationProvider.LoadOrDefault<LocalizationConfiguration>();
            ValidateOutputPath();
        }

        private void ValidateOutputPath ()
        {
            var localizationRoot = config.Loader.PathPrefix;

            availableLocalizations.Clear();
            if (LocaleFolderPath != null && Directory.Exists(LocaleFolderPath) && LocaleFolderPath.EndsWithFast(localizationRoot))
            {
                var locales = Directory.GetDirectories(LocaleFolderPath).Select(Path.GetFileName);
                foreach (var locale in locales)
                    if (LanguageTags.ContainsTag(locale))
                        availableLocalizations.Add(locale);
            }

            targetTag = LocaleFolderPath?.GetAfter("/");
            sourceTag = SourceScriptsPath?.GetAfterFirst($"{localizationRoot}/")?.GetBefore("/");
            outputPathValid = localizationRootSelected || (LocaleFolderPath?.GetBeforeLast("/")?.EndsWith(localizationRoot) ?? false) &&
                LanguageTags.ContainsTag(targetTag) && targetTag != config.SourceLocale;
            scriptSourcePathValid = LanguageTags.ContainsTag(sourceTag) && targetTag != sourceTag && sourceTag != config.SourceLocale;
            if (!scriptSourcePathValid) sourceTag = config.SourceLocale;
            if (outputPathValid)
            {
                targetLanguage = localizationRootSelected ? string.Join(", ", availableLocalizations) : LanguageTags.GetLanguageByTag(targetTag);
                sourceLanguage = scriptSourcePathValid ? LanguageTags.GetLanguageByTag(sourceTag) : $"{LanguageTags.GetLanguageByTag(config.SourceLocale)} (source)";
            }
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Localization", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate localization resources; see `Localization` guide for usage instructions.", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                LocaleFolderPath = EditorGUILayout.TextField(localeFolderPathContent, LocaleFolderPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    LocaleFolderPath = EditorUtility.OpenFolderPanel("Locale Folder Path", "", "");
            }
            if (outputPathValid)
                EditorGUILayout.HelpBox(targetLanguage, MessageType.None, false);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                SourceScriptsPath = EditorGUILayout.TextField(sourceScriptsPathContent, SourceScriptsPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    SourceScriptsPath = EditorUtility.OpenFolderPanel("Locale Folder Path", "", "");
            }
            if (outputPathValid)
                EditorGUILayout.HelpBox(sourceLanguage, MessageType.None, false);

            EditorGUILayout.Space();

            if (localizeManagedText)
                using (new EditorGUILayout.HorizontalScope())
                {
                    SourceManagedTextPath = EditorGUILayout.TextField(sourceManagedTextPathContent, SourceManagedTextPath);
                    if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                        SourceManagedTextPath = EditorUtility.OpenFolderPanel("Locale Folder Path", "", "");
                }

            EditorGUILayout.Space();

            localizeManagedText = EditorGUILayout.Toggle(localizeManagedTextContent, localizeManagedText);
            tryUpdate = EditorGUILayout.Toggle(tryUpdateContent, tryUpdate);
            if (tryUpdate)
                warnUntranslated = EditorGUILayout.Toggle(warnUntranslatedContent, warnUntranslated);
            GUILayout.FlexibleSpace();

            EditorGUILayout.HelpBox(wordCount >= 0 ? $"Total word count in the localization documents: {wordCount}." : "Total word count in the localization documents will be printed here after the documents are generated.", MessageType.Info);

            if (!outputPathValid)
            {
                if (targetTag == config.SourceLocale)
                    EditorGUILayout.HelpBox($"You're trying to create a `{targetTag}` localization, which is equal to the project source locale. That is not allowed; see `Localization` guide for more info.", MessageType.Error);
                else EditorGUILayout.HelpBox("Locale Folder path is not valid. Make sure it points to the localization root or a subdirectory with name equal to one of the supported language tags.", MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!outputPathValid);
            if (GUILayout.Button("Generate Localization Resources", GUIStyles.NavigationButton))
                GenerateLocalizationResources();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        private void GenerateLocalizationResources ()
        {
            EditorUtility.DisplayProgressBar(progressBarTitle, "Reading source documents...", 0f);

            try
            {
                if (localizationRootSelected)
                {
                    foreach (var locale in availableLocalizations)
                        DoGenerate(Path.Combine(LocaleFolderPath, locale));
                }
                else DoGenerate(LocaleFolderPath);

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate localization resources: {e.Message}");
            }

            EditorUtility.ClearProgressBar();
            Repaint();

            void DoGenerate (string localeFolderPath)
            {
                LocalizeScripts(localeFolderPath);
                if (localizeManagedText) LocalizeManagedText(localeFolderPath);
            }
        }

        private string BuildLocalizationHeader (string localeFolderPath, string sourceName)
        {
            var targetTag = Path.GetFileName(localeFolderPath);
            var targetLanguage = LanguageTags.GetLanguageByTag(targetTag);
            return $"{Identifiers.CommentLine} {sourceLanguage.Remove(" (source)")} <{sourceTag}> to {targetLanguage} <{targetTag}> localization document for {sourceName}\n";
        }

        private void LocalizeScripts (string localeFolderPath)
        {
            wordCount = 0;

            var scriptPathPrefix = ProjectConfigurationProvider.LoadOrDefault<ScriptsConfiguration>().Loader.PathPrefix;
            var outputDirPath = $"{localeFolderPath}/{scriptPathPrefix}";
            if (!Directory.Exists(outputDirPath))
                Directory.CreateDirectory(outputDirPath);

            if (scriptSourcePathValid) // Generate based on already generated docs for an other language.
            {
                var sourceScriptPaths = Directory.GetFiles(SourceScriptsPath, "*.nani", SearchOption.AllDirectories);
                for (int pathIdx = 0; pathIdx < sourceScriptPaths.Length; pathIdx++)
                {
                    var sourceScriptPath = sourceScriptPaths[pathIdx];
                    var sourceScriptName = Path.GetFileNameWithoutExtension(sourceScriptPath);
                    var progress = pathIdx / (float)sourceScriptPaths.Length;

                    EditorUtility.DisplayProgressBar(progressBarTitle, $"Processing `{sourceScriptName}`...", progress);

                    var sourceScript = AssetDatabase.LoadAssetAtPath<Script>(PathUtils.AbsoluteToAssetPath(sourceScriptPath));
                    var sourceTextLines = Parsing.ScriptParser.SplitText(File.ReadAllText(sourceScriptPath));
                    Debug.Assert(sourceScript.Lines.Count == sourceTextLines.Length);

                    var outputPath = sourceScriptPath.Replace(LanguageTags.GetTagByLanguage(sourceLanguage), LanguageTags.GetTagByLanguage(targetLanguage));
                    var outputBuilder = new StringBuilder(BuildLocalizationHeader(localeFolderPath, $"`{sourceScriptName}` naninovel script"));
                    var shouldUpdate = ShouldAppendExisting(outputPath, out var existingScript, out var existingTextLines);
                    var currentLabelText = default(string);
                    for (int lineIdx = 1; lineIdx < sourceScript.Lines.Count; lineIdx++) // Starting from one to skip title comment.
                    {
                        var sourceLine = sourceScript.Lines[lineIdx];
                        if (sourceLine is LabelScriptLine labelLine)
                        {
                            if (shouldUpdate)
                                AppendExistingLines(existingScript, existingTextLines, currentLabelText, outputBuilder);

                            currentLabelText = labelLine.LabelText;
                            outputBuilder.AppendLine();
                            outputBuilder.AppendLine($"{Identifiers.LabelLine} {labelLine.LabelText}");
                            continue;
                        }
                        if (!IsLineLocalizable(sourceLine)) continue; // Whether the line contains some actual translation.

                        outputBuilder.AppendLine($"{Identifiers.CommentLine} {sourceTextLines[lineIdx]}"); // Copy the source text to comments.

                        CountWords(sourceTextLines[lineIdx]);
                    }

                    File.WriteAllText(outputPath, outputBuilder.ToString());
                }
            }
            else // Generate based on source scripts.
            {
                var sourceScriptPaths = EditorResources.LoadOrDefault().GetAllRecords(scriptPathPrefix).Select(kv => AssetDatabase.GUIDToAssetPath(kv.Value)).ToArray();
                for (int pathIdx = 0; pathIdx < sourceScriptPaths.Length; pathIdx++)
                {
                    var sourceScriptPath = sourceScriptPaths[pathIdx];
                    if (!File.Exists(sourceScriptPath)) continue;
                    var sourceScriptName = Path.GetFileNameWithoutExtension(sourceScriptPath);
                    var progress = pathIdx / (float)sourceScriptPaths.Length;

                    EditorUtility.DisplayProgressBar(progressBarTitle, $"Processing `{sourceScriptName}`...", pathIdx / (float)sourceScriptPaths.Length);

                    var sourceText = File.ReadAllText(sourceScriptPath);
                    var sourceTextLines = Parsing.ScriptParser.SplitText(sourceText);
                    var sourceScript = AssetDatabase.LoadAssetAtPath<Script>(sourceScriptPath);
                    Debug.Assert(sourceScript.Lines.Count == sourceTextLines.Length);

                    var outputPath = $"{outputDirPath}/{sourceScript.Name}.nani";
                    var outputBuilder = new StringBuilder(BuildLocalizationHeader(localeFolderPath, $"`{sourceScriptName}` naninovel script"));
                    var shouldUpdate = ShouldAppendExisting(outputPath, out var existingScript, out var existingTextLines);
                    for (int lineIdx = 0; lineIdx < sourceScript.Lines.Count; lineIdx++)
                    {
                        var sourceLine = sourceScript.Lines[lineIdx];
                        if (!IsLineLocalizable(sourceLine)) continue;

                        var sourceTextLine = sourceTextLines[lineIdx];
                        outputBuilder.AppendLine();
                        outputBuilder.AppendLine($"{Identifiers.LabelLine} {sourceLine.LineHash}");
                        outputBuilder.AppendLine($"{Identifiers.CommentLine} {sourceTextLine}");

                        if (shouldUpdate)
                            AppendExistingLines(existingScript, existingTextLines, sourceLine.LineHash, outputBuilder);
                        CountWords(sourceTextLine);
                    }

                    File.WriteAllText(outputPath, outputBuilder.ToString());
                }
            }

            EditorUtility.ClearProgressBar();

            bool IsLineLocalizable (ScriptLine line)
            {
                if (line is GenericTextScriptLine genericLine)
                    return genericLine.InlinedCommands.Any(c => c is Command.ILocalizable);
                if (line is CommandScriptLine commandLine)
                    return commandLine.Command is Command.ILocalizable;
                return false;
            }

            // string.Split(null) will delimit by whitespace chars; `default(char[])` is used to prevent ambiguity in case of overloads.
            void CountWords (string value) => wordCount += value.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Length;

            bool ShouldAppendExisting (string outputPath, out Script existingScript, out string[] existingTextLines)
            {
                existingScript = tryUpdate ? AssetDatabase.LoadAssetAtPath<Script>(PathUtils.AbsoluteToAssetPath(outputPath)) : null;
                existingTextLines = existingScript ? Parsing.ScriptParser.SplitText(File.ReadAllText(outputPath)) : null;
                return tryUpdate && existingScript;
            }

            void AppendExistingLines (Script existingScript, string[] existingLines, string lineHash, StringBuilder builder)
            {
                var locIdx = existingScript.GetLineIndexForLabel(lineHash);
                if (locIdx == -1) return;

                var appended = false;
                var hashIndex = locIdx;
                while (existingScript.Lines.IsIndexValid(locIdx + 1))
                {
                    locIdx++;
                    var existingLine = existingScript.Lines[locIdx];
                    if (existingLine is CommentScriptLine || existingLine is EmptyScriptLine) continue;
                    if (existingLine is LabelScriptLine) break;
                    builder.AppendLine(existingLines[locIdx]);
                    appended = true;
                }
                if (!appended && warnUntranslated)
                    Debug.LogWarning($"`{AssetDatabase.GetAssetPath(existingScript)}` localization script is missing translation at line #{hashIndex + 1}.");
            }
        }

        private void LocalizeManagedText (string localeFolderPath)
        {
            if (!Directory.Exists(SourceManagedTextPath)) return;

            var outputPath = $"{localeFolderPath}/{ProjectConfigurationProvider.LoadOrDefault<ManagedTextConfiguration>().Loader.PathPrefix}";
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var filePaths = Directory.GetFiles(SourceManagedTextPath, "*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                var docPath = filePaths[i];
                var docText = File.ReadAllText(docPath);
                var category = Path.GetFileNameWithoutExtension(docPath);
                var targetPath = Path.Combine(outputPath, $"{category}.txt");
                var records = ManagedTextUtils.ParseDocument(docText, category);

                if (tryUpdate && File.Exists(targetPath))
                {
                    var existingText = File.ReadAllText(targetPath);
                    var existingRecords = ManagedTextUtils.ParseDocument(existingText, category);

                    foreach (var existingRecord in existingRecords)
                    {
                        if (!records.Remove(existingRecord)) continue;
                        records.Add(existingRecord);
                    }
                }

                var outputBuilder = new StringBuilder();
                outputBuilder.AppendLine(BuildLocalizationHeader(localeFolderPath, $"`{category}` managed text document"));
                foreach (var record in records)
                    outputBuilder.AppendLine(record.ToDocumentTextLine());
                File.WriteAllText(targetPath, outputBuilder.ToString());
            }
        }
    }
}
