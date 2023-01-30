// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEditor;

namespace Naninovel
{
    [CustomEditor(typeof(PlayScript))]
    public class PlayScriptEditor : Editor
    {
        private SerializedProperty scriptName;
        private SerializedProperty scriptText;
        private SerializedProperty playOnAwake;
        private SerializedProperty disableWaitInput;

        private void OnEnable ()
        {
            scriptName = serializedObject.FindProperty("scriptName");
            scriptText = serializedObject.FindProperty("scriptText");
            playOnAwake = serializedObject.FindProperty("playOnAwake");
            disableWaitInput = serializedObject.FindProperty("disableWaitInput");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(scriptName);
            if (string.IsNullOrEmpty(scriptName.stringValue))
            {
                EditorGUILayout.PropertyField(scriptText);
                EditorGUILayout.PropertyField(disableWaitInput);
            }
            EditorGUILayout.PropertyField(playOnAwake);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
