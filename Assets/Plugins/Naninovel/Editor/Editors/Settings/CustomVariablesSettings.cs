// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Naninovel
{
    public class CustomVariablesSettings : ConfigurationSettings<CustomVariablesConfiguration>
    {
        private const float headerLeftMargin = 5;
        private const float paddingWidth = 10;

        private static readonly GUIContent nameContent = new GUIContent("Name", "Name of the custom variable. Add `G_` or `g_` prefix to make it a global variable.");
        private static readonly GUIContent valueContent = new GUIContent("Value (expression)", "A script expression to initialize the variable.");

        private ReorderableList predefinedVariablesList;

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(CustomVariablesConfiguration.PredefinedVariables)] = DrawPredefinedVariablesEditor;
            return drawers;
        }
        
        private void DrawPredefinedVariablesEditor (SerializedProperty property)
        {
            var label = EditorGUI.BeginProperty(Rect.zero, null, property);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Always check list's serialized object parity with the inspected object.
            if (predefinedVariablesList is null || predefinedVariablesList.serializedProperty.serializedObject != SerializedObject)
                InitializePredefinedVariablesList();

            predefinedVariablesList.DoLayoutList();

            EditorGUI.EndProperty();
        }

        private void InitializePredefinedVariablesList ()
        {
            predefinedVariablesList = new ReorderableList(SerializedObject, SerializedObject.FindProperty(nameof(CustomVariablesConfiguration.PredefinedVariables)), true, true, true, true);
            predefinedVariablesList.drawHeaderCallback = DrawPredefinedVariablesListHeader;
            predefinedVariablesList.drawElementCallback = DrawPredefinedVariablesListElement;
        }

        private void DrawPredefinedVariablesListHeader (Rect rect)
        {
            var propertyRect = new Rect(headerLeftMargin + rect.x, rect.y, (rect.width / 2f) - paddingWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propertyRect, nameContent);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.LabelField(propertyRect, valueContent);
        }

        private void DrawPredefinedVariablesListElement (Rect rect, int index, bool isActive, bool isFocused)
        {
            var propertyRect = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, (rect.width / 2f) - paddingWidth, EditorGUIUtility.singleLineHeight);

            var elementProperty = predefinedVariablesList.serializedProperty.GetArrayElementAtIndex(index);
            var nameProperty = elementProperty.FindPropertyRelative("name");
            var valueProperty = elementProperty.FindPropertyRelative("value");

            EditorGUI.PropertyField(propertyRect, nameProperty, GUIContent.none);

            propertyRect.x += propertyRect.width + paddingWidth;

            EditorGUI.PropertyField(propertyRect, valueProperty, GUIContent.none);
        }
    }
}
