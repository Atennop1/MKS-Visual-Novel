// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class CharactersSettings : OrthoActorManagerSettings<CharactersConfiguration, ICharacterActor, CharacterMetadata>
    {
        protected override string HelpUri => "guide/characters.html";
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override MetadataEditor<ICharacterActor, CharacterMetadata> MetadataEditor { get; } = new CharacterMetadataEditor();

        private static readonly GUIContent AvatarsEditorContent = new GUIContent("Avatar Resources",
            "Use 'CharacterId/Appearance' name to map avatar texture to a character appearance. Use 'CharacterId/Default' to map a default avatar to the character.");

        private bool avatarsEditorExpanded;

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(CharactersConfiguration.AvatarLoader)] = DrawAvatarsEditor;
            drawers[nameof(CharactersConfiguration.ArrangeRange)] = DrawArrangeRangeEditor;
            return drawers;
        }

        private string GetTooltip ()
        {
            if (AllowMultipleResources)
                return $"Use `@char {EditedActorId}.%name%` in naninovel scripts to show the character with selected appearance.";
            return $"Use `@char {EditedActorId}` in naninovel scripts to show this character.";
        }

        private void DrawAvatarsEditor (SerializedProperty avatarsLoaderProperty)
        {
            EditorGUILayout.PropertyField(avatarsLoaderProperty);

            avatarsEditorExpanded = EditorGUILayout.Foldout(avatarsEditorExpanded, AvatarsEditorContent, true);
            if (!avatarsEditorExpanded) return;
            ResourcesEditor.DrawGUILayout(Configuration.AvatarLoader.PathPrefix, AllowRename, Configuration.AvatarLoader.PathPrefix, null, typeof(Texture2D),
                "Use `@char CharacterID avatar:%name%` in naninovel scripts to assign selected avatar texture for the character.");
        }

        private void DrawArrangeRangeEditor (SerializedProperty serializedProperty)
        {
            EditorGUILayout.PropertyField(serializedProperty);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var value = serializedProperty.vector2Value;
            GUILayout.Space(EditorGUIUtility.labelWidth);
            EditorGUILayout.MinMaxSlider(ref value.x, ref value.y, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
                serializedProperty.vector2Value = value;
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Naninovel/Resources/Characters")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
