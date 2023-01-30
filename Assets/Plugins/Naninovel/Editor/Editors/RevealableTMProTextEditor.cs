// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.UI;
using UnityEditor;

namespace Naninovel
{
    [CustomEditor(typeof(RevealableTMProText), true)]
    [CanEditMultipleObjects]
    public class RevealableTMProTextEditor : NaninovelTMProTextEditor
    {
        private SerializedProperty revealFadeWidth;
        private SerializedProperty slideClipRect;
        private SerializedProperty italicSlantAngle;
        private SerializedProperty clipRectScale;

        protected override void OnEnable ()
        {
            base.OnEnable();

            revealFadeWidth = serializedObject.FindProperty("revealFadeWidth");
            slideClipRect = serializedObject.FindProperty("slideClipRect");
            italicSlantAngle = serializedObject.FindProperty("italicSlantAngle");
            clipRectScale = serializedObject.FindProperty("clipRectScale");
        }

        protected override void DrawAdditionalInspectorGUI ()
        {
            EditorGUILayout.LabelField("Revealing", EditorStyles.boldLabel);
            ++EditorGUI.indentLevel;
            {
                EditorGUILayout.PropertyField(revealFadeWidth);
                EditorGUILayout.PropertyField(slideClipRect);
                EditorGUILayout.PropertyField(italicSlantAngle);
                EditorGUILayout.PropertyField(clipRectScale);
            }
            --EditorGUI.indentLevel;
        }
    }
}
