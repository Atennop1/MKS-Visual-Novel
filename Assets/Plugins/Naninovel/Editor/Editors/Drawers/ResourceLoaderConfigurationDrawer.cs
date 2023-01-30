// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Naninovel
{
    [CustomPropertyDrawer(typeof(ResourceLoaderConfiguration))]
    public class ResourceLoaderConfigurationDrawer : PropertyDrawer
    {
        // Drawer instances are shared between properties being drawn,
        // so using this class to isolate property-specific data.
        private class DrawerState
        {
            public ReorderableList ReorderableList;
            public SerializedProperty PathProperty;
            public int LastSelectedIndex;
        }

        private static readonly string[] providerTypes;
        private static readonly GUIContent[] providerTypeLabels;

        private static readonly GUIContent listContent = new GUIContent("Providers List", "Provider types to use, in order." +
            "\n\nBuilt-in options:" +
            "\n • Addressable — For assets managed via the Addressable Asset System." +
            "\n • Project — For assets stored in project's `Resources` folders." +
            "\n • Local — For assets stored on a local file system." +
            "\n • GoogleDrive — For assets stored remotely on a Google Drive account.");

        private Dictionary<string, DrawerState> stateMap = new Dictionary<string, DrawerState>();
        private ReorderableList reorderableList;
        private SerializedProperty loaderProperty;
        private SerializedProperty providerListProperty;
        private SerializedProperty pathProperty;
        private int lastSelectedIndex;
        private GUIContent label;

        static ResourceLoaderConfigurationDrawer ()
        {
            var types = Engine.Types
                .Where(t => !t.IsAbstract && t.GetInterfaces().Contains(typeof(IResourceProvider)) && t != typeof(EditorResourceProvider) && t != typeof(VirtualResourceProvider))
                .Select(t => t.AssemblyQualifiedName).ToList();
            if (!types.Contains(ResourceProviderConfiguration.AddressableTypeName))
                types.Add(ResourceProviderConfiguration.AddressableTypeName);
            if (!types.Contains(ResourceProviderConfiguration.GoogleDriveTypeName))
                types.Add(ResourceProviderConfiguration.GoogleDriveTypeName);
            providerTypes = types.ToArray();
            providerTypeLabels = providerTypes.Select(s => s.GetBefore(",")).Select(s => new GUIContent(s)).ToArray();
        }

        public override void OnGUI (Rect rect, SerializedProperty property, GUIContent label)
        {
            this.label = label;
            var state = InitializeStateFor(property);

            OnGUI(rect);

            state.LastSelectedIndex = lastSelectedIndex;
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            var state = InitializeStateFor(property);

            if (!loaderProperty.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            var list = state.ReorderableList;
            return EditorGUIUtility.singleLineHeight * 3
                 + list.headerHeight
                 + (list.count <= 0 ? EditorGUIUtility.singleLineHeight : list.elementHeight * list.count)
                 + list.footerHeight;
        }

        private void OnGUI (Rect rect)
        {
            if (loaderProperty is null) return;

            label = EditorGUI.BeginProperty(rect, label, loaderProperty);

            loaderProperty.isExpanded = EditorGUI.Foldout(ToSingleLine(rect), loaderProperty.isExpanded, label, true);

            if (loaderProperty.isExpanded)
            {
                EditorGUI.indentLevel++;
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(ToSingleLine(rect), pathProperty);
                rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var listRect = EditorGUI.PrefixLabel(rect, listContent);
                reorderableList.DoList(listRect);
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private static Rect ToSingleLine (Rect rect) => new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

        private DrawerState InitializeStateFor (SerializedProperty property)
        {
            var key = property.propertyPath;
            stateMap.TryGetValue(key, out var state);

            // Ensure the cached SerializedProperty is synchronized.
            if (state is null || state.ReorderableList.serializedProperty.serializedObject != property.serializedObject)
            {
                if (state is null)
                    state = new DrawerState();

                state.PathProperty = property.FindPropertyRelative(nameof(ResourceLoaderConfiguration.PathPrefix));
                var providerListProperty = property.FindPropertyRelative(nameof(ResourceLoaderConfiguration.ProviderTypes));
                state.ReorderableList = new ReorderableList(property.serializedObject, providerListProperty, true, false, true, true);
                state.ReorderableList.headerHeight = 4;
                state.ReorderableList.drawElementCallback = DrawListElement;
                state.ReorderableList.onAddCallback = HandleListElementAdded;
                state.ReorderableList.onRemoveCallback = HandleListElementRemoved;
                state.ReorderableList.onSelectCallback = HandleListElementSelected;
                state.ReorderableList.onReorderCallback = HandleListElementReordered;

                stateMap[key] = state;
            }

            loaderProperty = property;
            reorderableList = state.ReorderableList;
            providerListProperty = state.ReorderableList.serializedProperty;
            pathProperty = state.PathProperty;
            lastSelectedIndex = state.LastSelectedIndex;
            reorderableList.index = lastSelectedIndex;

            return state;
        }

        private void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
        {
            var initialIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            var elementProperty = providerListProperty.GetArrayElementAtIndex(index);

            //EditorGUI.PropertyField(rect, elementProperty, GUIContent.none);
            var curIndex = ArrayUtility.IndexOf(providerTypes, elementProperty.stringValue ?? string.Empty);
            var newIndex = EditorGUI.Popup(rect, curIndex, providerTypeLabels);
            elementProperty.stringValue = providerTypes.IsIndexValid(newIndex) ? providerTypes[newIndex] : string.Empty;

            EditorGUI.indentLevel = initialIndent;
        }

        private void HandleListElementAdded (ReorderableList list)
        {
            if (providerListProperty.hasMultipleDifferentValues)
            {
                Debug.LogWarning("Loader property doesn't support mixed values; state of the affected objects might be undefined.");

                // When increasing a multi-selection array using Serialized Property data can be overwritten if there is mixed values.
                // The Serialization system applies the Serialized data of one object, to all other objects in the selection.
                // We handle this case here, by creating a SerializedObject for each object.
                foreach (var targetObject in providerListProperty.serializedObject.targetObjects)
                {
                    var tempSerializedObject = new SerializedObject(targetObject);
                    var listenerArrayProperty = tempSerializedObject.FindProperty(providerListProperty.propertyPath);
                    listenerArrayProperty.arraySize += 1;
                    tempSerializedObject.ApplyModifiedProperties();
                }
                providerListProperty.serializedObject.SetIsDifferentCacheDirty();
                providerListProperty.serializedObject.Update();
                list.index = list.serializedProperty.arraySize - 1;
            }
            else ReorderableList.defaultBehaviours.DoAddButton(list);

            lastSelectedIndex = list.index;
        }

        private void HandleListElementRemoved (ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            lastSelectedIndex = list.index;
        }

        private void HandleListElementSelected (ReorderableList list)
        {
            lastSelectedIndex = list.index;
        }

        private void HandleListElementReordered (ReorderableList list)
        {
            lastSelectedIndex = list.index;
        }
    }
}
