// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEditor;
using UnityEditor.UI;

namespace Naninovel
{
    [CustomEditor(typeof(UI.RevealableUIText), true)]
    [CanEditMultipleObjects]
    public class RevealableUITextEditor : TextEditor
    {
        private SerializedProperty text;
        private SerializedProperty fontData;
        private SerializedProperty revealFadeWidth;
        private SerializedProperty slideClipRect;
        private SerializedProperty italicSlantAngle;

        protected override void OnEnable ()
        {
            base.OnEnable();

            text = serializedObject.FindProperty("m_Text");
            fontData = serializedObject.FindProperty("m_FontData");
            revealFadeWidth = serializedObject.FindProperty("revealFadeWidth");
            slideClipRect = serializedObject.FindProperty("slideClipRect");
            italicSlantAngle = serializedObject.FindProperty("italicSlantAngle");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(text);
            EditorGUILayout.PropertyField(fontData);

            EditorGUILayout.LabelField("Revealing", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(revealFadeWidth);
                EditorGUILayout.PropertyField(slideClipRect);
                EditorGUILayout.PropertyField(italicSlantAngle);
            }
            --EditorGUI.indentLevel;

            AppearanceControlsGUI();
            RaycastControlsGUI();

            serializedObject.ApplyModifiedProperties();
        }
    } 
}
