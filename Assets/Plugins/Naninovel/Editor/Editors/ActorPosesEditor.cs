// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public static class ActorPosesEditor
    {
        private static readonly GUIContent nameContent = new GUIContent("Pose Name",
            "Identifier of the pose. Use it instead of appearance in naninovel script commands to apply the pose.");
        private static readonly GUIContent zeroOpacityTintContent = new GUIContent("Tint Color",
            "Tint opacity is zero; actor won't be visible. In case you want to hide the actor, set `Visible` to false instead.");
        private static readonly Color warnColor = new Color(1, 1, .5f);
        private static readonly HashSet<string> overrides = new HashSet<string>();

        public static void Draw (SerializedProperty posesProperty)
        {
            var label = EditorGUI.BeginProperty(Rect.zero, null, posesProperty);
            posesProperty.isExpanded = EditorGUILayout.Foldout(posesProperty.isExpanded, label, true);
            if (posesProperty.isExpanded)
            {
                EditorGUI.indentLevel++;
                posesProperty.arraySize = EditorGUILayout.IntField("Size", posesProperty.arraySize);
                for (int i = 0; i < posesProperty.arraySize; i++)
                    DrawPose(posesProperty.GetArrayElementAtIndex(i));
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private static void DrawPose (SerializedProperty poseProperty)
        {
            var label = EditorGUI.BeginProperty(Rect.zero, null, poseProperty);
            poseProperty.isExpanded = EditorGUILayout.Foldout(poseProperty.isExpanded, label, true);
            if (poseProperty.isExpanded)
            {
                EditorGUI.indentLevel++;
                var nameProperty = poseProperty.FindPropertyRelative("name");
                EditorGUILayout.PropertyField(nameProperty, nameContent);
                var stateProperty = poseProperty.FindPropertyRelative("actorState");
                var overridesProperty = poseProperty.FindPropertyRelative("overriddenProperties");
                FeedOverridesFrom(overridesProperty);
                DrawState(stateProperty, overridesProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }

        private static void DrawState (SerializedProperty stateProperty, SerializedProperty overridesProperty)
        {
            var property = stateProperty.Copy();
            var endProperty = property.GetEndProperty();
            property.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(property, endProperty)) break;
                EditorGUILayout.BeginHorizontal();
                var @override = DrawOverrideToggleFor(property.name, overridesProperty);
                EditorGUI.BeginDisabledGroup(!@override);
                if (@override && property.name.EqualsFast("tintColor") && property.colorValue.a == 0)
                    DrawTintColorWithWarning(property);
                else EditorGUILayout.PropertyField(property, true);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            } while (property.NextVisible(false));
        }

        private static void FeedOverridesFrom (SerializedProperty overridesProperty)
        {
            overrides.Clear();
            for (int i = 0; i < overridesProperty.arraySize; i++)
                overrides.Add(overridesProperty.GetArrayElementAtIndex(i).stringValue);
        }

        private static void SyncOverridesTo (SerializedProperty overridesProperty)
        {
            overridesProperty.ClearArray();
            var index = 0;
            foreach (var name in overrides)
            {
                overridesProperty.InsertArrayElementAtIndex(index);
                overridesProperty.GetArrayElementAtIndex(index).stringValue = name;
                index++;
            }
        }

        private static void DrawTintColorWithWarning (SerializedProperty property)
        {
            var initialColor = GUI.color;
            GUI.color = warnColor;
            EditorGUILayout.PropertyField(property, zeroOpacityTintContent);
            GUI.color = initialColor;
        }

        private static bool DrawOverrideToggleFor (string propertyName, SerializedProperty overridesProperty)
        {
            EditorGUI.BeginChangeCheck();
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
            rect.width = 45; // Otherwise the toggle is not registering clicks.
            var @override = EditorGUI.Toggle(rect, overrides.Contains(propertyName));
            if (EditorGUI.EndChangeCheck())
            {
                if (@override) overrides.Add(propertyName);
                else overrides.Remove(propertyName);
                SyncOverridesTo(overridesProperty);
            }
            return @override;
        }
    }
}
