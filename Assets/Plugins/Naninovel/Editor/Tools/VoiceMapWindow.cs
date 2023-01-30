// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class VoiceMapWindow : EditorWindow
    {
        private class MapItem
        {
            public GUIContent Label;
            public string ClipGuid;
            public AudioClip ClipObject;
        }

        private static readonly GUIContent scriptContent = new GUIContent("Script", "Naninovel script for which to edit auto voice associations.");
        private static readonly Color invalidObjectColor = new Color(1, .8f, .8f);
        private static readonly Type clipType = typeof(AudioClip);
        private static readonly Type scriptType = typeof(Script);

        private readonly Dictionary<string, MapItem> map = new Dictionary<string, MapItem>();
        private AudioConfiguration audioConfig;
        private EditorResources editorResources;
        private Script script;
        private Vector2 scrollPos;

        [MenuItem("Naninovel/Tools/Voice Map")]
        public static void OpenWindow ()
        {
            GetWindow<VoiceMapWindow>("Voice Map", true);
        }

        private void OnEnable ()
        {
            editorResources = EditorResources.LoadOrDefault();
            audioConfig = ProjectConfigurationProvider.LoadOrDefault<AudioConfiguration>();
            UpdateSelectedScript();
        }

        private void OnDisable ()
        {
            SaveResources();
        }

        private void OnGUI ()
        {
            if (!ObjectUtils.IsValid(script) && Selection.activeObject is Script selectedScript)
            {
                script = selectedScript;
                UpdateSelectedScript();
            }

            EditorGUI.BeginChangeCheck();
            script = EditorGUILayout.ObjectField(scriptContent, script, scriptType, false) as Script;
            if (EditorGUI.EndChangeCheck()) UpdateSelectedScript();

            if (!ObjectUtils.IsValid(script))
            {
                EditorGUILayout.HelpBox("Select a script to start mapping the voice clips.", MessageType.None, false);
                return;
            }

            EditorGUILayout.Space();

            if (map.Count == 0)
            {
                EditorGUILayout.HelpBox("Selected script doesn't contain any commands to associate voice clips with. Voice clips can be associated with generic text lines (containing a text to print) and with @print commands.", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var kv in map)
            {
                var hash = kv.Key;
                var data = kv.Value;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(data.Label);
                var rect = GUILayoutUtility.GetLastRect();
                rect.width = EditorGUIUtility.currentViewWidth;
                var hovered = rect.Contains(Event.current.mousePosition);

                GUILayout.Space(5);

                if (hovered || ObjectUtils.IsValid(data.ClipObject) || string.IsNullOrEmpty(data.ClipGuid))
                {
                    if (!ObjectUtils.IsValid(data.ClipObject) && !string.IsNullOrEmpty(data.ClipGuid))
                    {
                        var assetPath = AssetDatabase.GUIDToAssetPath(data.ClipGuid);
                        data.ClipObject = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                        if (!ObjectUtils.IsValid(data.ClipObject)) data.ClipGuid = string.Empty;
                    }

                    EditorGUI.BeginChangeCheck();
                    var initialGuidColor = GUI.color;
                    if (!ObjectUtils.IsValid(data.ClipObject)) GUI.color = invalidObjectColor;
                    data.ClipObject = EditorGUILayout.ObjectField(GUIContent.none, data.ClipObject, clipType, false) as AudioClip;
                    GUI.color = initialGuidColor;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (!ObjectUtils.IsValid(data.ClipObject)) data.ClipGuid = string.Empty;
                        else AssetDatabase.TryGetGUIDAndLocalFileIdentifier(data.ClipObject, out data.ClipGuid, out long _);
                    }
                }
                else
                {
                    var path = AssetDatabase.GUIDToAssetPath(data.ClipGuid);
                    if (path.Contains("/")) path = path.GetAfter("/");
                    if (path.Length > 30) path = path.Substring(path.Length - 30);
                    EditorGUILayout.LabelField(path, EditorStyles.objectField);
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void UpdateSelectedScript ()
        {
            SaveResources();
            map.Clear();

            if (!ObjectUtils.IsValid(script)) return;

            var extractedCommands = script.ExtractCommands()
                .Where(c => c is Commands.PrintText)
                .Cast<Commands.PrintText>();

            foreach (var cmd in extractedCommands)
            {
                if (!cmd?.Text?.HasValue ?? true) continue;

                var hash = AudioConfiguration.GetAutoVoiceClipPath(cmd);
                var label = $"#{cmd.PlaybackSpot.LineNumber}.{cmd.PlaybackSpot.InlineIndex} ";
                if (!string.IsNullOrEmpty(cmd.AuthorId)) label += $"{cmd.AuthorId}: ";
                label += cmd.Text.DynamicValue ? cmd.Text.DynamicValueText : cmd.Text.Value;
                var clipPath = PathUtils.Combine(audioConfig.VoiceLoader.PathPrefix, hash);
                var clipGuid = editorResources.GetGuidByPath(clipPath);
                var mapItem = new MapItem { Label = new GUIContent(label, label), ClipGuid = clipGuid };
                map[hash] = mapItem;
            }
        }

        private void SaveResources ()
        {
            if (!ObjectUtils.IsValid(audioConfig)) return;

            var category = audioConfig.VoiceLoader.PathPrefix;

            foreach (var kv in map)
            {
                var hash = kv.Key;
                var data = kv.Value;

                editorResources.RemoveAllRecordsWithPath(category, hash, category);
                if (!string.IsNullOrEmpty(data.ClipGuid))
                    editorResources.AddRecord(category, category, hash, data.ClipGuid);
            }

            EditorUtility.SetDirty(editorResources);
            AssetDatabase.SaveAssets();
        }
    }
}
