// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    [CustomEditor(typeof(LayeredActorBehaviour), true)]
    public class LayeredActorBehaviourEditor : Editor
    {
        private const string mapFieldName = "compositionMap";
        private static readonly GUIContent previewContent = new GUIContent("Preview Composition");
        private static readonly GUIContent pasteContent = new GUIContent("Paste Current Composition");

        private void OnEnable ()
        {
            EditorApplication.contextualPropertyMenu += HandlePropertyContextMenu;
        }

        private void OnDisable ()
        {
            EditorApplication.contextualPropertyMenu -= HandlePropertyContextMenu;
        }

        private void HandlePropertyContextMenu (GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Generic ||
                !property.propertyPath.Contains($"{mapFieldName}.Array.data[")) return;

            var propertyCopy = property.Copy();
            var targetObj = propertyCopy.serializedObject.targetObject as LayeredActorBehaviour;
            if (!targetObj || !ObjectUtils.IsEditedInPrefabMode(targetObj.gameObject))
            {
                menu.AddDisabledItem(previewContent);
                menu.AddDisabledItem(pasteContent);
                return;
            }

            menu.AddItem(previewContent, false, () => PreviewComposition(propertyCopy, targetObj));
            menu.AddItem(pasteContent, false, () => PasteComposition(propertyCopy, targetObj));
        }

        private static void PreviewComposition (SerializedProperty property, LayeredActorBehaviour target)
        {
            target.RebuildLayers();
            var composition = GetComposition(property, target);
            target.ApplyComposition(composition);
            property.serializedObject.Update();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        private static void PasteComposition (SerializedProperty property, LayeredActorBehaviour target)
        {
            property.FindPropertyRelative("Composition").stringValue = target.Composition;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static string GetComposition (SerializedProperty property, LayeredActorBehaviour target)
        {
            if (!TryGetIndex(property, out var index)) return null;
            return target.GetCompositionMap().Values.ElementAt(index);
        }

        private static bool TryGetIndex (SerializedProperty property, out int index)
        {
            index = property.propertyPath.GetBetween($"{mapFieldName}.Array.data[", "]")?.AsInvariantInt() ?? -1;
            return index > -1;
        }
    }
}
