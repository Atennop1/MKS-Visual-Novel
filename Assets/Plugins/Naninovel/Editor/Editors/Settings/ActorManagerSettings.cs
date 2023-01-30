// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Derive from this class to create custom editors for <see cref="Configuration"/> assets of <see cref="IActorManager"/> services.
    /// </summary>
    /// <typeparam name="TConfig">Type of the configuration asset this editor is built for.</typeparam>
    /// <typeparam name="TActor">Type of the actor this editor is built for.</typeparam>
    /// <typeparam name="TMeta">Type of the actor meta the actor manager uses.</typeparam>
    public abstract class ActorManagerSettings<TConfig, TActor, TMeta> : ResourcefulSettings<TConfig>
        where TConfig : Configuration
        where TActor : IActor
        where TMeta : ActorMetadata
    {
        protected SerializedProperty MetadataMapProperty { get; private set; }
        protected string EditedActorId => EditingMetadata ? MetadataMapEditor.SelectedActorId : null;
        protected TMeta EditedMetadata => EditingMetadata ? MetadataMapEditor.EditedMetadataProperty.GetGenericValue<TMeta>() : DefaultMetadata;
        protected TMeta DefaultMetadata { get; private set; }
        protected bool EditingMetadata => MetadataMapEditor.EditedMetadataProperty != null;
        protected GUIContent FromMetaButtonLabel { get; private set; }
        protected MetadataMapEditor MetadataMapEditor { get; private set; }
        protected abstract MetadataEditor<TActor, TMeta> MetadataEditor { get; }
        protected virtual string MetadataMapPropertyName => "Metadata";
        protected virtual string DefaultMetadataPropertyName => "DefaultMetadata";
        protected virtual HashSet<string> LockedActorIds => null;
        protected virtual bool AllowMultipleResources => ActorImplementations.TryGetResourcesAttribute(EditedMetadata.Implementation, out var attr) && attr.AllowMultiple;
        protected override Type ResourcesTypeConstraint => ActorImplementations.TryGetResourcesAttribute(EditedMetadata.Implementation, out var attr) ? attr.TypeConstraint : null;
        protected override string ResourcesPathPrefix => AllowMultipleResources ? $"{EditedMetadata.Loader.PathPrefix}/{EditedActorId}" : EditedMetadata.Loader.PathPrefix;
        protected override string ResourcesCategoryId => EditedMetadata.GetResourceCategoryId();
        protected override string ResourceName => AllowMultipleResources ? null : EditedActorId;

        private HashSet<string> lockedActorIds;

        public override void OnActivate (string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            lockedActorIds = LockedActorIds;
            FromMetaButtonLabel = new GUIContent($"< Back To {EditorTitle} List");
            MetadataMapProperty = SerializedObject.FindProperty(MetadataMapPropertyName);
            MetadataMapEditor = new MetadataMapEditor(SerializedObject, MetadataMapProperty, typeof(TMeta), EditorTitle, lockedActorIds);
            DefaultMetadata = SerializedObject.FindProperty(DefaultMetadataPropertyName).GetGenericValue<TMeta>();

            MetadataMapEditor.OnElementModified += HandleMetadataElementModified;
        }

        public override void OnDeactivate ()
        {
            base.OnDeactivate();

            if (MetadataMapEditor != null)
                MetadataMapEditor.OnElementModified -= HandleMetadataElementModified;
        }

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[MetadataMapPropertyName] = null;
            drawers[DefaultMetadataPropertyName] = property => {
                var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
                if (!property.isExpanded) return;
                EditorGUI.indentLevel++;
                MetadataEditor.Draw(property, EditedMetadata);
                EditorGUI.indentLevel--;
            };
            return drawers;
        }

        protected override void DrawConfigurationEditor ()
        {
            if (ShowResourcesEditor)
            {
                if (EditingMetadata)
                {
                    if (GUILayout.Button(FromMetaButtonLabel, GUIStyles.NavigationButton))
                        MetadataMapEditor.ResetEditedMetadata();
                    else
                    {
                        EditorGUILayout.Space();
                        DrawMetaEditor(MetadataMapEditor.EditedMetadataProperty);
                    }
                }
                else
                {
                    if (GUILayout.Button(FromResourcesButtonContent, GUIStyles.NavigationButton))
                        ShowResourcesEditor = false;
                    else
                    {
                        EditorGUILayout.Space();
                        MetadataMapEditor.DrawGUILayout();
                    }
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

        protected virtual void DrawMetaEditor (SerializedProperty metaProperty)
        {
            var actorTitle = MetadataMapEditor.SelectedActorId.InsertCamel();

            EditorGUILayout.LabelField($"{actorTitle} Metadata", EditorStyles.boldLabel);
            MetadataEditor.Draw(metaProperty, EditedMetadata);

            EditorGUILayout.Space();

            if (ResourcesTypeConstraint != null)
            {
                EditorGUILayout.LabelField(actorTitle + (AllowMultipleResources ? " Resources" : " Resource"), EditorStyles.boldLabel);
                ResourcesEditor.DrawGUILayout(ResourcesCategoryId, AllowRename, ResourcesPathPrefix, ResourceName, ResourcesTypeConstraint, ResourcesSelectionTooltip);
            }

            // Return to meta list when pressing return key and no text fields are edited.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Backspace && !EditorGUIUtility.editingTextField)
            {
                MetadataMapEditor.ResetEditedMetadata();
                Event.current.Use();
            }
        }

        protected virtual void HandleMetadataElementModified (MetadataMapEditor.ElementModifiedArgs args)
        {
            // Remove resources category associated with the removed actor.
            if (args.ModificationType == MetadataMapEditor.ElementModificationType.Remove)
            {
                var categoryId = args.Metadata.GetResourceCategoryId();
                ResourcesEditor.RemoveCategory(categoryId);
            }
        }
    }
}
