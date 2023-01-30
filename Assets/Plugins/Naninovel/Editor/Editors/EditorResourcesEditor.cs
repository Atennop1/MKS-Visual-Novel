// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Editor for resources stored in <see cref="EditorResources"/> asset.
    /// </summary>
    public class EditorResourcesEditor
    {
        private const float headerLeftMargin = 5;
        private const float paddingWidth = 5;

        private const string categoryIdPropertyName = "Id";
        private const string categoryResourcesPropertyName = "Resources";
        private static readonly GUIContent pathContent = new GUIContent("Name  <i><size=10>(hover for hotkey info)</size></i>", "Local path (relative to the path prefix) of the resource.\n\nHotkeys:\n • Delete key — Remove selected record.\n • Up/Down keys — Navigate selected records.\n\nIt's possible to add resources in batch by drag-dropping multiple assets or folders to an area below the list (appears when compatible assets are dragged).");
        private static readonly GUIContent objectContent = new GUIContent("Object", "Object of the resource.\n\nThe assigned objects are loaded only when hovered for better performance.");
        private static readonly GUIContent invalidObjectContent = new GUIContent("", "Assign a valid object or remove the record.");
        private static readonly GUIContent duplicateNameContent = new GUIContent("", "Duplicate names could cause issues. Change name for one of the affected records.");
        private static readonly Color invalidObjectColor = new Color(1, .8f, .8f);
        private static readonly Color duplicateNameColor = new Color(1, 1, .8f);

        private bool singleMode => resourceName != null;

        private readonly SerializedObject serializedObject;
        private readonly SerializedProperty categoriesProperty;
        private readonly Dictionary<string, UnityEngine.Object> cachedObjects;
        private readonly Dictionary<string, ReorderableList> reorderableLists;
        private readonly HashSet<string> names = new HashSet<string>();
        private string categoryId, pathPrefix, resourceName, selectionTooltip, selectedName;
        private bool allowRename;
        private Type typeConstraint;

        public EditorResourcesEditor (EditorResources editorResources)
        {
            serializedObject = new SerializedObject(editorResources);
            categoriesProperty = serializedObject.FindProperty("resourceCategories");
            cachedObjects = new Dictionary<string, UnityEngine.Object>();
            reorderableLists = new Dictionary<string, ReorderableList>();
        }

        /// <summary>
        /// Draws the editor GUI using layout system.
        /// </summary>
        /// <param name="categoryId">Category ID of the edited resources.</param>
        /// <param name="allowRename">Whether to allow renaming resource names (local paths).</param>
        /// <param name="pathPrefix">When provided, will add the path before the edited resources names (it won't be visible for the user).</param>
        /// <param name="resourceName">When provided, will draw a single-element editor for resource with the provided name.</param>
        /// <param name="typeConstraint">Type constraint to apply for resource objects.</param>
        /// <param name="selectionTooltip">The tooltip template for selected records; %name% tags will be replaced with the name of the selected resource.</param>
        public void DrawGUILayout (string categoryId, bool allowRename = true, string pathPrefix = null, string resourceName = null, Type typeConstraint = null, string selectionTooltip = null)
        {
            serializedObject.Update();

            this.categoryId = categoryId;
            this.allowRename = allowRename;
            this.pathPrefix = pathPrefix;
            this.resourceName = resourceName;
            this.typeConstraint = typeConstraint;
            this.selectionTooltip = selectionTooltip;

            var reorderableList = GetReorderableListForCategory(categoryId);
            var serializedProperty = reorderableList.serializedProperty;

            // Setup single mode in case the array size is not right.
            if (singleMode && serializedProperty.arraySize != 1)
            {
                serializedProperty.arraySize = 1;
                reorderableList.index = 0;
            }

            reorderableList.DoLayoutList();

            DrawSelectionTooltip();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Attempts to remove a resources category with the provided ID and all the associated records.
        /// Use this instead of <see cref="EditorResources.RemoveCategory(string)"/> to correctly handle undo/redo while this editor is initialized.
        /// </summary>
        /// <returns>Whether the category has been found and deleted.</returns>
        public bool RemoveCategory (string categoryId)
        {
            reorderableLists.Remove(categoryId);

            var elementIndexToRemove = -1;
            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                var categoryProperty = categoriesProperty.GetArrayElementAtIndex(i);
                var categoryIdProperty = categoryProperty.FindPropertyRelative(categoryIdPropertyName);
                if (categoryIdProperty.stringValue == categoryId)
                {
                    elementIndexToRemove = i;
                    break;
                }
            }
            if (elementIndexToRemove == -1) return false;

            categoriesProperty.DeleteArrayElementAtIndex(elementIndexToRemove);
            serializedObject.ApplyModifiedProperties();
            return true;
        }

        private ReorderableList InitializeListForCategory (string categoryId)
        {
            var resourcesProperty = default(SerializedProperty);

            // Attempt to find an existing category.
            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                var categoryProperty = categoriesProperty.GetArrayElementAtIndex(i);
                var categoryIdProperty = categoryProperty.FindPropertyRelative(categoryIdPropertyName);
                if (categoryIdProperty.stringValue == categoryId)
                {
                    resourcesProperty = categoryProperty.FindPropertyRelative(categoryResourcesPropertyName);
                    break;
                }
            }

            // Create new category if not found.
            if (resourcesProperty is null)
            {
                categoriesProperty.arraySize++;
                var categoryProperty = categoriesProperty.GetArrayElementAtIndex(categoriesProperty.arraySize - 1);
                categoryProperty.FindPropertyRelative(categoryIdPropertyName).stringValue = categoryId;
                resourcesProperty = categoryProperty.FindPropertyRelative(categoryResourcesPropertyName);
                resourcesProperty.ClearArray();
            }

            var reorderableList = new ReorderableList(serializedObject, resourcesProperty, true, true, true, true);
            reorderableList.drawHeaderCallback = DrawHeader;
            reorderableList.drawElementCallback = DrawElement;
            reorderableList.drawFooterCallback = DrawFooter;
            reorderableList.onAddCallback = HandleElementAdded;
            reorderableList.onRemoveCallback = HandleElementRemoved;
            reorderableList.onSelectCallback = HandleElementSelected;
            return reorderableList;
        }

        private ReorderableList GetReorderableListForCategory (string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentNullException(nameof(categoryId));

            if (reorderableLists.TryGetValue(categoryId, out var existingList))
            {
                // Always check list's serialized object parity with the inspected object.
                if (existingList != null && existingList.serializedProperty.serializedObject == serializedObject)
                    return existingList;
            }

            var newList = InitializeListForCategory(categoryId);
            reorderableLists[categoryId] = newList;
            return newList;
        }

        private SerializedProperty GetNamePropertyAt (int index, ReorderableList list) => list?.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name");
        private SerializedProperty GetPathPrefixPropertyAt (int index, ReorderableList list) => list?.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("pathPrefix");
        private SerializedProperty GetGuidPropertyAt (int index, ReorderableList list) => list?.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("guid");

        private void DrawHeader (Rect rect)
        {
            var propertyRect = new Rect(headerLeftMargin + rect.x, rect.y, (rect.width / 2f) - paddingWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propertyRect, pathContent, GUIStyles.RichLabelStyle);
            propertyRect.x += propertyRect.width + paddingWidth;
            EditorGUI.LabelField(propertyRect, objectContent);
        }

        private void DrawElement (Rect rect, int index, bool selected, bool focused)
        {
            if (index == 0) names.Clear();

            var reorderableList = GetReorderableListForCategory(categoryId);
            if (index < 0 || index >= reorderableList.serializedProperty.arraySize) return;

            var elementNameProperty = GetNamePropertyAt(index, reorderableList);
            var elementPathPrefixProperty = GetPathPrefixPropertyAt(index, reorderableList);
            var elementGuidProperty = GetGuidPropertyAt(index, reorderableList);
            var elementName = elementNameProperty.stringValue;
            var elementPathPrefix = elementPathPrefixProperty.stringValue;
            var elementGuid = elementGuidProperty.stringValue;
            var elementHovered = rect.Contains(Event.current.mousePosition);

            // Delete record when pressing delete key while an element is selected, but not editing text field.
            if (reorderableList.index == index && Event.current.type == EventType.KeyDown &&
                Event.current.keyCode == KeyCode.Delete && selected && !EditorGUIUtility.editingTextField)
            {
                HandleElementRemoved(reorderableList);
                Event.current.Use();
                return;
            }

            // Select list row when clicking on the inlined properties.
            if (Event.current.type == EventType.MouseDown && elementHovered)
            {
                reorderableList.index = index;
                reorderableList.onSelectCallback?.Invoke(reorderableList);
            }

            EditorGUI.BeginChangeCheck();

            var propertyRect = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, (rect.width / 2f) - paddingWidth, EditorGUIUtility.singleLineHeight);

            // Set resource path prefix property.
            if (elementPathPrefix != pathPrefix)
                elementPathPrefixProperty.stringValue = pathPrefix;

            // Draw resource name (local path) property.
            var initialNameColor = GUI.color;
            var duplicate = names.Contains(elementName);
            if (duplicate) GUI.color = duplicateNameColor;
            EditorGUI.LabelField(propertyRect, duplicate ? duplicateNameContent : GUIContent.none);
            EditorGUI.BeginDisabledGroup(singleMode || !allowRename);
            var oldNameValue = elementName;
            var newNameValue = EditorGUI.TextField(propertyRect, GUIContent.none, singleMode ? resourceName : oldNameValue);
            if (newNameValue.Contains("\\")) // Force-replace backward slashes.
                newNameValue = newNameValue.Replace("\\", "/");
            if (oldNameValue != newNameValue)
                elementNameProperty.stringValue = newNameValue;
            EditorGUI.EndDisabledGroup();
            GUI.color = initialNameColor;
            names.Add(newNameValue);

            propertyRect.x += propertyRect.width + paddingWidth;

            // Draw resource GUID property.
            if (elementHovered || IsAssetLoadedOrCached(elementGuid) || string.IsNullOrEmpty(elementGuid))
            {
                var objectType = typeConstraint ?? typeof(UnityEngine.Object);
                var oldObjectValue = string.IsNullOrEmpty(elementGuid) ? default : GetCachedOrLoadAssetByGuid(elementGuid, objectType);

                var initialGuidColor = GUI.color;
                if (!ObjectUtils.IsValid(oldObjectValue) && !string.IsNullOrEmpty(newNameValue))
                {
                    GUI.color = invalidObjectColor;
                    EditorGUI.LabelField(propertyRect, invalidObjectContent);
                }
                var newObjectValue = EditorGUI.ObjectField(propertyRect, GUIContent.none, oldObjectValue, objectType, false);
                GUI.color = initialGuidColor;

                if (oldObjectValue != newObjectValue)
                {
                    if (newObjectValue is null)
                        elementGuidProperty.stringValue = string.Empty;
                    else
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newObjectValue, out var newGuidValue, out long _);
                        elementGuidProperty.stringValue = newGuidValue;
                    }
                }

                // Auto-assign default name when object is set, but name is empty or rename is not allowed.
                if (newObjectValue != null && (string.IsNullOrEmpty(newNameValue) || !allowRename))
                    elementNameProperty.stringValue = newObjectValue.name;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(elementGuid);
                if (path.Contains("/")) path = path.GetAfter("/");
                if (path.Length > 30) path = path.Substring(path.Length - 30);
                EditorGUI.LabelField(propertyRect, path, EditorStyles.objectField);
            }

            if (EditorGUI.EndChangeCheck())
                UpdateSelectedName();
        }

        private void DrawFooter (Rect rect)
        {
            // Don't draw footer when in single resource mode.
            if (singleMode) return;

            // Allow dragging objects to the footer area to add them in batch.
            if (CanAddDraggedObjects())
            {
                rect.height += 20;
                rect.y += 5;
                GUILayoutUtility.GetRect(rect.width, 30);
                EditorGUI.HelpBox(rect, "Drop the assets or folders here to add the resources in batch.", MessageType.Info);

                if (rect.Contains(Event.current.mousePosition))
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (Event.current.type == EventType.DragPerform && rect.Contains(Event.current.mousePosition))
                {
                    AddDraggedObjects();
                    DragAndDrop.AcceptDrag();
                }
            }
            else ReorderableList.defaultBehaviours.DrawFooter(rect, GetReorderableListForCategory(categoryId));
        }

        private void DrawSelectionTooltip ()
        {
            if (Event.current.type == EventType.KeyDown) return; // To prevent https://forum.unity.com/threads/135021/#post-914872 when undoing list items removal.
            if (CanAddDraggedObjects() || string.IsNullOrEmpty(selectionTooltip) || string.IsNullOrEmpty(selectedName)) return;
            var message = selectionTooltip.Replace("%name%", selectedName);
            SelectableTooltip.Draw(message);
        }

        private void HandleElementAdded (ReorderableList list)
        {
            list.serializedProperty.arraySize++;
            list.index = list.serializedProperty.arraySize - 1;

            // Reset values of the added element (they're duplicated from a previous element by default).
            GetNamePropertyAt(list.index, list).stringValue = string.Empty;
            GetPathPrefixPropertyAt(list.index, list).stringValue = string.Empty;
            GetGuidPropertyAt(list.index, list).stringValue = string.Empty;

            UpdateSelectedName();
        }

        private void HandleElementRemoved (ReorderableList list)
        {
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            if (list.index >= list.serializedProperty.arraySize - 1)
                list.index = list.serializedProperty.arraySize - 1;

            UpdateSelectedName();
        }

        private void HandleElementSelected (ReorderableList list)
        {
            UpdateSelectedName();
        }

        private void UpdateSelectedName ()
        {
            var list = GetReorderableListForCategory(categoryId);
            var selectionValid = list.count > 0 && list.index > -1 && list.index < list.count;
            selectedName = selectionValid ? GetNamePropertyAt(list.index, list)?.stringValue : null;
        }

        private bool CanBeAdded (UnityEngine.Object obj)
        {
            return obj != null && (typeConstraint is null || obj.GetType() == typeConstraint || ProjectWindowUtil.IsFolder(obj.GetInstanceID()));
        }

        private bool CanAddDraggedObjects ()
        {
            if (DragAndDrop.objectReferences.Length == 0) return false;

            foreach (var obj in DragAndDrop.objectReferences)
                if (!CanBeAdded(obj))
                    return false;
            return true;
        }

        private void AddDraggedObjects ()
        {
            foreach (var obj in DragAndDrop.objectReferences)
                AddObject(obj);

            var list = GetReorderableListForCategory(categoryId);
            list.index = list.serializedProperty.arraySize - 1;
            UpdateSelectedName();
        }

        private void AddObject (UnityEngine.Object obj, string name = null)
        {
            void AddFolder (UnityEngine.Object folderObj)
            {
                var folderPath = AssetDatabase.GetAssetPath(folderObj);
                var guids = AssetDatabase.FindAssets(null, new[] { folderPath }).DistinctBy(p => p);
                var objects = new Dictionary<string, UnityEngine.Object>();
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetDatabase.IsValidFolder(assetPath)) continue;
                    var assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeConstraint ?? typeof(UnityEngine.Object));
                    if (assetObj is null || !CanBeAdded(assetObj)) continue;
                    var resourceName = assetPath.GetAfterFirst(folderPath + "/").GetBeforeLast(".");
                    objects.Add(resourceName, assetObj);
                }

                foreach (var kv in objects)
                    AddObject(kv.Value, kv.Key);
            }

            if (!CanBeAdded(obj)) return;

            if (ProjectWindowUtil.IsFolder(obj.GetInstanceID()))
            {
                AddFolder(obj);
                return;
            }

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var newGuidValue, out long instanceId);
            if (string.IsNullOrEmpty(newGuidValue)) return;

            var list = GetReorderableListForCategory(categoryId);
            list.serializedProperty.arraySize += 1;
            var index = list.serializedProperty.arraySize - 1;

            GetGuidPropertyAt(index, list).stringValue = newGuidValue;
            GetNamePropertyAt(index, list).stringValue = name ?? obj.name;
            GetPathPrefixPropertyAt(index, list).stringValue = pathPrefix;
        }

        private UnityEngine.Object GetCachedOrLoadAssetByGuid (string guid, Type type)
        {
            if (cachedObjects.ContainsKey(guid))
                return cachedObjects[guid];

            var obj = EditorUtils.LoadAssetByGuid(guid, type);
            if (obj is null) return null;

            cachedObjects[guid] = obj;

            return obj;
        }

        private bool IsAssetLoadedOrCached (string guid) => cachedObjects.ContainsKey(guid) || AssetDatabase.IsMainAssetAtPathLoaded(AssetDatabase.GUIDToAssetPath(guid));
    }
}
