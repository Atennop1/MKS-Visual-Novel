// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Editor for <see cref="ActorMetadataMap{TMeta}"/>-derived properties.
    /// </summary>
    public class MetadataMapEditor
    {
        public enum ElementModificationType { Add, Remove }

        public class ElementModifiedArgs : EventArgs
        {
            /// <summary>
            /// Metadata (value) of the modified map element.
            /// </summary>
            public readonly ActorMetadata Metadata;
            /// <summary>
            /// Type of the modification done.
            /// </summary>
            public readonly ElementModificationType ModificationType;

            public ElementModifiedArgs (ActorMetadata metadata, ElementModificationType modificationType)
            {
                Metadata = metadata;
                ModificationType = modificationType;
            }
        }

        /// <summary>
        /// Invoked when a map element is modified (added, removed or edited).
        /// </summary>
        public Action<ElementModifiedArgs> OnElementModified;

        public string SelectedActorId => reorderableList is null ? null : GetIdPropertyAt(reorderableList.index)?.stringValue;
        public SerializedProperty EditedMetadataProperty { get; private set; }

        private const float paddingWidth = 5;
        private const float buttonWidth = 45;
        private static readonly GUIContent editButtonLabel = new GUIContent(Engine.LoadInternalResource<Texture2D>("EditMetaIcon"), "Edit actor metadata.");

        private readonly SerializedObject serializedObject;
        private readonly SerializedProperty idsProperty;
        private readonly SerializedProperty metasProperty;
        private readonly GUIContent listHeaderLabel;
        private readonly Type metaType;
        private readonly HashSet<string> lockedIds;
        private readonly Dictionary<int, SerializedProperty> idsCache = new Dictionary<int, SerializedProperty>();
        private readonly Dictionary<int, SerializedProperty> metasCache = new Dictionary<int, SerializedProperty>();
        private ReorderableList reorderableList;

        public MetadataMapEditor (SerializedObject serializedObject, SerializedProperty mapProperty, Type metaType, string actorsLabel = "Actors", HashSet<string> lockedIds = null)
        {
            this.serializedObject = serializedObject;
            this.metaType = metaType;
            this.lockedIds = lockedIds;
            idsProperty = mapProperty.FindPropertyRelative("ids");
            metasProperty = mapProperty.FindPropertyRelative("metas");
            listHeaderLabel = new GUIContent($"{actorsLabel} List  <i><size=10>(hover for hotkey info)</size></i>", "Hotkeys:\n • Double-click or Enter key — Edit actor metadata (while a record is selected).\n • Backspace key — Return to metadata list (while editing a metadata).\n • Delete key — Remove selected record.\n • Up/Down keys — Navigate selected records.");
        }

        /// <summary>
        /// Draws the editor GUI using layout system.
        /// </summary>
        public void DrawGUILayout ()
        {
            // Always check list's serialized object parity with the inspected object.
            if (reorderableList is null || reorderableList.serializedProperty.serializedObject != serializedObject)
                InitializeList();

            reorderableList.DoLayoutList();
        }

        /// <summary>
        /// Attempts to start editing metadata mapped to the provided actor ID.
        /// </summary>
        /// <returns>Whether the requested metadata was found and is being edited.</returns>
        public bool SelectEditedMetadata (string actorId)
        {
            if (reorderableList is null)
                InitializeList();

            for (int i = 0; i < idsProperty.arraySize; i++)
            {
                if (idsProperty.GetArrayElementAtIndex(i).stringValue == actorId)
                {
                    reorderableList.index = i;
                    EditMetaProperty(GetMetaPropertyAt(i));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Un-selects any currently edited metadata (if any).
        /// </summary>
        public void ResetEditedMetadata () => EditedMetadataProperty = null;

        private void InitializeList ()
        {
            reorderableList = new ReorderableList(serializedObject, idsProperty, true, true, true, true);
            reorderableList.drawHeaderCallback = DrawListHeader;
            reorderableList.drawElementCallback = DrawListElement;
            reorderableList.onAddCallback = HandleListElementAdded;
            reorderableList.onRemoveCallback = HandleListElementRemoved;
            reorderableList.onReorderCallbackWithDetails = HandleListElementReordered;
            reorderableList.onCanAddCallback = CanAddListElement;
            reorderableList.onCanRemoveCallback = CanRemoveListElement;
            ClearPropertiesCache();
        }

        private void ClearPropertiesCache ()
        {
            idsCache.Clear();
            metasCache.Clear();
        }

        private SerializedProperty GetIdPropertyAt (int index)
        {
            if (idsCache.ContainsKey(index)) 
                return idsCache[index];

            var property = idsProperty.GetArrayElementAtIndexOrNull(index);
            idsCache[index] = property;
            return property;
        }

        private SerializedProperty GetMetaPropertyAt (int index)
        {
            if (metasCache.ContainsKey(index)) 
                return metasCache[index];

            var property = metasProperty.GetArrayElementAtIndexOrNull(index);
            metasCache[index] = property;
            return property;
        }

        private void DrawListHeader (Rect rect)
        {
            EditorGUI.LabelField(rect, listHeaderLabel, GUIStyles.RichLabelStyle);
        }

        private void DrawListElement (Rect rect, int index, bool selected, bool focused)
        {
            if (index < 0 || index >= reorderableList.serializedProperty.arraySize) return;

            var idProperty = GetIdPropertyAt(index);
            var metaProperty = GetMetaPropertyAt(index);
            var initialIdValue = idProperty.stringValue;

            // Select list row when clicking on the inlined properties.
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                reorderableList.index = index;
                reorderableList.onSelectCallback?.Invoke(reorderableList);

                // Edit meta when double-clicking.
                if (Event.current.clickCount >= 2 && !string.IsNullOrEmpty(initialIdValue))
                    EditMetaProperty(metaProperty);
            }

            // Edit meta when pressing enter key and the element is selected.
            if (reorderableList.index == index && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
                EditorGUIUtility.textFieldHasSelection && !string.IsNullOrEmpty(initialIdValue))
            {
                EditMetaProperty(metaProperty);
                Event.current.Use();
            }

            // Delete record when pressing delete key while an element is selected, but not editing text field.
            if (reorderableList.index == index && Event.current.type == EventType.KeyDown && 
                Event.current.keyCode == KeyCode.Delete && selected && !EditorGUIUtility.editingTextField)
            { 
                HandleListElementRemoved(reorderableList);
                Event.current.Use();
                return;
            }

            EditorGUI.BeginDisabledGroup(lockedIds != null && lockedIds.Contains(initialIdValue));
            var propertyRect = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, rect.width - paddingWidth - buttonWidth, EditorGUIUtility.singleLineHeight);
            var newIdValue = EditorGUI.TextField(propertyRect, GUIContent.none, initialIdValue);
            EditorGUI.EndDisabledGroup();

            // Make sure the new value is unique.
            if (newIdValue != initialIdValue && !IsIdUnique(newIdValue))
                newIdValue = initialIdValue;

            if (newIdValue != initialIdValue)
                idProperty.stringValue = newIdValue;

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(newIdValue));
            propertyRect.x += propertyRect.width + paddingWidth;
            propertyRect.width = buttonWidth;
            if (GUI.Button(propertyRect, editButtonLabel, EditorStyles.miniButton))
                EditMetaProperty(metaProperty);
            EditorGUI.EndDisabledGroup();
        }

        private void HandleListElementAdded (ReorderableList list)
        {
            idsProperty.arraySize++;
            metasProperty.arraySize++;

            list.index = idsProperty.arraySize - 1;

            GetIdPropertyAt(list.index).stringValue = string.Empty;

            // Unity doesn't provide API to set generic values to serialized properties; using a hack with reflection.
            serializedObject.ApplyModifiedProperties();
            var metaProperty = GetMetaPropertyAt(list.index);
            
            var defaultMeta = Activator.CreateInstance(metaType);
            metaProperty.SetGenericValue(defaultMeta);
            serializedObject.Update();

            OnElementModified?.Invoke(new ElementModifiedArgs(metaProperty.GetGenericValue<ActorMetadata>(), ElementModificationType.Add));
        }

        private void HandleListElementRemoved (ReorderableList list)
        {
            var removedElementId = GetIdPropertyAt(list.index).stringValue;
            var removedElementMetadata = GetMetaPropertyAt(list.index).GetGenericValue<ActorMetadata>();

            idsProperty.DeleteArrayElementAtIndex(list.index);
            metasProperty.DeleteArrayElementAtIndex(list.index);

            if (list.index >= idsProperty.arraySize - 1)
                list.index = idsProperty.arraySize - 1;
            
            ClearPropertiesCache();

            OnElementModified?.Invoke(new ElementModifiedArgs(removedElementMetadata, ElementModificationType.Remove));
        }

        private void HandleListElementReordered (ReorderableList list, int oldIndex, int newIndex)
        {
            metasProperty.MoveArrayElement(oldIndex, newIndex);
            ClearPropertiesCache();
        }

        private bool CanAddListElement (ReorderableList list)
        {
            for (int i = 0; i < reorderableList.count; i++)
                if (string.IsNullOrWhiteSpace(GetIdPropertyAt(i).stringValue)) return false;
            return true;
        }

        private bool CanRemoveListElement (ReorderableList list)
        {
            var actorId = GetIdPropertyAt(list.index).stringValue;
            return lockedIds is null || !lockedIds.Contains(actorId);
        }

        private bool IsIdUnique (string key)
        {
            for (int i = 0; i < reorderableList.count; i++)
                if (GetIdPropertyAt(i).stringValue == key) return false;
            return true;
        }

        private void EditMetaProperty (SerializedProperty metaProperty)
        {
            EditedMetadataProperty = metaProperty;
            EditorGUIUtility.editingTextField = false;
        }
    }
}
