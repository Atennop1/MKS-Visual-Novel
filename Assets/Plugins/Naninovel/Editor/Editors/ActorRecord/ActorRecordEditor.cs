// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEditor;

namespace Naninovel
{
    public abstract class ActorRecordEditor<TEditor, TMeta, TConfig> : Editor
        where TEditor : MetadataEditor, new()
        where TMeta : ActorMetadata, new()
        where TConfig : ActorManagerConfiguration<TMeta>
    {
        private TMeta metadata;
        private SerializedProperty metadataProperty;
        private MetadataEditor metadataEditor;
        private TConfig configuration;
        private SerializedObject serializedConfiguration;

        public override void OnInspectorGUI ()
        {
            serializedObject.Update();
            metadataEditor.Draw(metadataProperty, metadata);
            DrawResourcesTooltip(metadata);
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                WriteToConfiguration();
            }
        }

        private void DrawResourcesTooltip (ActorMetadata metadata)
        {
            if (serializedObject.isEditingMultipleObjects) return;
            var path = ActorImplementations.TryGetResourcesAttribute(metadata.Implementation, out var attr) && attr.AllowMultiple
                ? $"Naninovel/{metadata.Loader.PathPrefix}/{target.name}/<RESOURCE_NAME>"
                : $"Naninovel/{metadata.Loader.PathPrefix}/{target.name}";
            SelectableTooltip.Draw($"Resource path: {path}",
                "To associate resources with this actor without using editor menus consult documentation for Addressable resource provider.");
        }

        private void OnEnable ()
        {
            metadataProperty = serializedObject.FindProperty("metadata");
            metadataEditor = new TEditor();
            metadata = ((ActorRecord<TMeta>)target).Metadata ?? new TMeta();
            configuration = ProjectConfigurationProvider.LoadOrDefault<TConfig>();
            serializedConfiguration = new SerializedObject(configuration);
            WriteToConfiguration();
        }

        private void WriteToConfiguration ()
        {
            foreach (var target in targets)
                if (!string.IsNullOrEmpty(target.name))
                    configuration.ActorMetadataMap[target.name] = ((ActorRecord<TMeta>)target).Metadata;
            serializedConfiguration.Update();
        }
    }

    [CustomEditor(typeof(CharacterRecord)), CanEditMultipleObjects]
    public class CharacterRecordEditor : ActorRecordEditor<CharacterMetadataEditor, CharacterMetadata, CharactersConfiguration> { }

    [CustomEditor(typeof(BackgroundRecord)), CanEditMultipleObjects]
    public class BackgroundRecordEditor : ActorRecordEditor<BackgroundMetadataEditor, BackgroundMetadata, BackgroundsConfiguration> { }

    [CustomEditor(typeof(TextPrinterRecord)), CanEditMultipleObjects]
    public class TextPrinterRecordEditor : ActorRecordEditor<TextPrinterMetadataEditor, TextPrinterMetadata, TextPrintersConfiguration> { }

    [CustomEditor(typeof(ChoiceHandlerRecord)), CanEditMultipleObjects]
    public class ChoiceHandlerRecordEditor : ActorRecordEditor<ChoiceHandlerMetadataEditor, ChoiceHandlerMetadata, ChoiceHandlersConfiguration> { }
}
