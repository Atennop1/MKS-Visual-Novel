// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    [CustomEditor(typeof(ScriptImporter))]
    public class ScriptImporterEditor : ScriptedImporterEditor
    {
        private const int previewLengthLimit = 5000;

        public override bool showImportedObject => false;
        public ScriptView VisualEditor { get; private set; }
        public Script ScriptAsset { get; private set; }

        private static readonly MethodInfo drawHeaderMethod;
        private static ScriptsConfiguration config;

        private string scriptText, previewContent;
        private float labelsHeight, gotosHeight;
        private GUIContent[] labelTags;
        private GUIContent[] gotoTags;

        static ScriptImporterEditor ()
        {
            drawHeaderMethod = typeof(Editor)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "DrawHeaderGUI" && m.GetParameters().Length == 2);
        }

        public override void OnEnable ()
        {
            base.OnEnable();

            if (config is null)
                config = ProjectConfigurationProvider.LoadOrDefault<ScriptsConfiguration>();
            ScriptAsset = assetTarget as Script;
            if (ScriptAsset == null) throw new Exception($"Failed to initialize script editor for `{assetTarget}`.");
            
            scriptText = File.ReadAllText(AssetDatabase.GetAssetPath(ScriptAsset));

            if (config.EnableVisualEditor)
            {
                VisualEditor = new ScriptView(config, ApplyRevertHackGUI, ApplyAndImportChecked);
                VisualEditor.GenerateForScript(scriptText, ScriptAsset);

                ScriptFileWatcher.OnModified += HandleScriptModified;
                return;
            }

            previewContent = scriptText;
            if (previewContent.Length > previewLengthLimit)
            {
                previewContent = previewContent.Substring(0, previewLengthLimit);
                previewContent += $"{Environment.NewLine}<...>";
            }

            labelTags = ScriptAsset.Lines.OfType<LabelScriptLine>().Select(l => new GUIContent($"# {l.LabelText}")).ToArray();
            gotoTags = ScriptAsset.ExtractCommands().OfType<Commands.Goto>()
                .Where(c => !string.IsNullOrEmpty(c.Path.Name))
                .Select(c => new GUIContent($"@goto {c.Path}")).ToArray();
        }

        public override void OnDisable ()
        {
            if (ObjectUtils.IsValid(ScriptAsset) && ScriptView.ScriptModified && 
                EditorUtility.DisplayDialog("Save changes?", $"Script `{ScriptAsset.Name}` has some un-saved changes. Would you like to keep or revert them?", "Save", "Revert"))
            {
                ApplyAndImportChecked();
            }

            base.OnDisable();

            if (VisualEditor != null)
                ScriptFileWatcher.OnModified -= HandleScriptModified;
            ScriptAsset = null;
        }

        public override VisualElement CreateInspectorGUI () => VisualEditor;

        public override bool HasModified () => false;

        protected override void Apply ()
        {
            base.Apply();

            if (VisualEditor is null) return;

            var scriptText = VisualEditor.GenerateText();
            var scriptPath = AssetDatabase.GetAssetPath(ScriptAsset);
            File.WriteAllText(scriptPath, scriptText);
            ScriptView.ScriptModified = false;
        }

        public override void OnInspectorGUI ()
        {
            var editorWasEnabled = GUI.enabled;
            GUI.enabled = true;

            if (labelTags != null && labelTags.Length > 0)
                DrawTags(labelTags, GUIStyles.ScriptLabelTag, ref labelsHeight);
            if (gotoTags != null && gotoTags.Length > 0)
                DrawTags(gotoTags, GUIStyles.ScriptGotoTag, ref gotosHeight);

            GUI.enabled = false;
            EditorGUILayout.LabelField(previewContent, EditorStyles.wordWrappedMiniLabel);

            GUI.enabled = editorWasEnabled;

            ApplyRevertHackGUI();
        }

        public void ApplyRevertHackGUI ()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(99999); // Hide the apply-revert buttons.
            ApplyRevertGUI(); // Required to prevent errors in the editor.
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(-EditorGUIUtility.singleLineHeight); // Hide empty line.
        }

        protected override void OnHeaderGUI ()
        {
            if (ScriptAsset == null || drawHeaderMethod == null) return;

            var headerRect = (Rect)drawHeaderMethod.Invoke(null, new object[] { this, ScriptAsset.name + (ScriptView.ScriptModified ? "*" : string.Empty) });
            using (new EditorGUI.DisabledScope(!ScriptView.ScriptModified))
            {
                var applyButtonRect = new Rect(new Vector2(headerRect.xMax - 98, headerRect.yMax - 26), new Vector2(48, 25));
                if (GUI.Button(applyButtonRect, "Apply", EditorStyles.miniButton))
                {
                    GUI.FocusControl(null);
                    ApplyAndImportChecked();
                }
                var revertButtonRect = new Rect(new Vector2(headerRect.xMax - 150, headerRect.yMax - 26), new Vector2(50, 25));
                if (GUI.Button(revertButtonRect, "Revert", EditorStyles.miniButton))
                {
                    GUI.FocusControl(null);
                    ResetValues();
                    if (ScriptAsset && VisualEditor != null)
                        VisualEditor.GenerateForScript(scriptText, ScriptAsset, true);
                }
            }
        }

        private void DrawTags (GUIContent[] tags, GUIStyle style, ref float heightBuffer)
        {
            const float horPadding = 5f;
            const float verPadding = 5f;

            if (Event.current.type == EventType.Repaint)
                heightBuffer = 0;

            GUILayout.Space(verPadding);

            // Create a rect to test how wide the label list can be.
            var widthProbeRect = GUILayoutUtility.GetRect(0, 10240, 0, 0);
            var rect = EditorGUILayout.GetControlRect();
            var controlHorPadding = rect.x;
            for (int i = 0; i < tags.Length; i++)
            {
                var content = tags[i];
                var labelSize = style.CalcSize(content);

                if (Event.current.type == EventType.Repaint && (rect.x + labelSize.x) >= widthProbeRect.xMax)
                {
                    var addHeight = GUIStyles.TagIcon.fixedHeight + verPadding;
                    rect.y += GUIStyles.TagIcon.fixedHeight + verPadding;
                    rect.x = controlHorPadding;
                    heightBuffer += addHeight;
                }

                var labelRect = new Rect(rect.x, rect.y, labelSize.x, labelSize.y);
                GUI.Label(labelRect, content, style);

                rect.x += labelSize.x + horPadding;
            }

            GUILayoutUtility.GetRect(0, heightBuffer);
        }

        private void ApplyAndImportChecked ()
        {
            if (!ScriptView.ScriptModified || !ObjectUtils.IsValid(ScriptAsset)) return;
            
            // Make sure the script actually changed before invoking `ApplyAndImport()`;
            // in case the generated script text will be the same as it was, Unity editor will 
            // fail internally and loose reference to the edited asset target.
            var scriptPath = AssetDatabase.GetAssetPath(ScriptAsset);
            var modifiedScriptText = VisualEditor.GenerateText();
            var savedScriptText = File.ReadAllText(scriptPath);
            if (modifiedScriptText == savedScriptText)
            {
                ScriptView.ScriptModified = false;
                return;
            }
            ApplyAndImport();
        }

        private void HandleScriptModified (string assetPath)
        {
            var curPath = AssetDatabase.GetAssetPath(ScriptAsset);
            if (curPath != assetPath) return;

            ScriptAsset = AssetDatabase.LoadAssetAtPath<Script>(assetPath);
            scriptText = File.ReadAllText(AssetDatabase.GetAssetPath(ScriptAsset));
            VisualEditor.GenerateForScript(scriptText, ScriptAsset);
        }
    }
}
