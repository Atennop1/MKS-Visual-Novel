// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Derive from this class to create custom combined editors for <see cref="Configuration"/> assets
    /// and associated resources stored in <see cref="Naninovel.EditorResources"/>.
    /// </summary>
    /// <typeparam name="TConfig">Type of the configuration asset this editor is built for.</typeparam>
    public abstract class ResourcefulSettings<TConfig> : ConfigurationSettings<TConfig> where TConfig : Configuration
    {
        protected static bool ShowResourcesEditor { get; set; }

        protected GUIContent ToResourcesButtonContent { get; private set; }
        protected GUIContent FromResourcesButtonContent { get; }
        protected EditorResources EditorResources { get; private set; }
        protected EditorResourcesEditor ResourcesEditor { get; private set; }
        protected abstract string ResourcesCategoryId { get; }
        protected virtual bool AllowRename => true;
        protected virtual string ResourcesPathPrefix => ResourcesCategoryId;
        protected virtual string ResourceName => null;
        protected virtual Type ResourcesTypeConstraint => null;
        protected virtual string ResourcesSelectionTooltip => null;

        protected ResourcefulSettings ()
        {
            FromResourcesButtonContent = new GUIContent("< Back To Configuration");
        }

        public override void OnActivate (string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            ToResourcesButtonContent = new GUIContent($"Manage {EditorTitle} Resources");
            EditorResources = EditorResources.LoadOrDefault();
            ResourcesEditor = new EditorResourcesEditor(EditorResources);
        }

        protected override void DrawConfigurationEditor ()
        {
            if (ShowResourcesEditor)
            {
                if (GUILayout.Button(FromResourcesButtonContent, GUIStyles.NavigationButton))
                    ShowResourcesEditor = false;
                else
                {
                    EditorGUILayout.Space();
                    ResourcesEditor.DrawGUILayout(ResourcesCategoryId, AllowRename, ResourcesPathPrefix, ResourceName, ResourcesTypeConstraint, ResourcesSelectionTooltip);
                }
            }
            else
            {
                DrawDefaultEditor();

                EditorGUILayout.Space();
                if (GUILayout.Button(ToResourcesButtonContent, GUIStyles.NavigationButton))
                    ShowResourcesEditor = true;
            }
        }

        protected static void OpenResourcesWindowImpl ()
        {
            ShowResourcesEditor = true;
            SettingsService.OpenProjectSettings(SettingsPath);
        }
    }
}
