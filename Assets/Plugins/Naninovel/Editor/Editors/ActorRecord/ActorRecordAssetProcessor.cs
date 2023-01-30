// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.IO;
using UnityEditor;

namespace Naninovel
{
    public class ActorRecordAssetProcessor : UnityEditor.AssetModificationProcessor
    {
        private static EditorResources editorResources = default;

        private static AssetDeleteResult OnWillDeleteAsset (string assetPath, RemoveAssetOptions options)
        {
            if (TryRemoveInFolder(assetPath)) return AssetDeleteResult.DidNotDelete;
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (!typeof(ActorRecord).IsAssignableFrom(assetType))
                return AssetDeleteResult.DidNotDelete;

            var record = AssetDatabase.LoadAssetAtPath<ActorRecord>(assetPath);
            var metadata = record.GetMetadata();
            RemoveActorRecord(record.name, metadata);
            RemoveAssociatedResources(metadata);
            AssetDatabase.SaveAssets();
            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset (string sourcePath, string destinationPath)
        {
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(sourcePath);
            if (!typeof(ActorRecord).IsAssignableFrom(assetType))
                return AssetMoveResult.DidNotMove;

            var sourceId = Path.GetFileNameWithoutExtension(sourcePath);
            var destinationId = Path.GetFileNameWithoutExtension(destinationPath);
            if (sourceId == destinationId) return AssetMoveResult.DidNotMove;
            var record = AssetDatabase.LoadAssetAtPath<ActorRecord>(sourcePath);
            var metadata = record.GetMetadata();
            MoveActorRecord(sourceId, destinationId, metadata);
            AssetDatabase.SaveAssets();
            return AssetMoveResult.DidNotMove;
        }

        private static bool TryRemoveInFolder (string assetPath)
        {
            if (!AssetDatabase.IsValidFolder(assetPath)) return false;
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(ActorRecord)}", new[] { assetPath }))
                OnWillDeleteAsset(AssetDatabase.GUIDToAssetPath(guid), default);
            return true;
        }

        private static void RemoveActorRecord (string actorId, ActorMetadata meta)
        {
            var config = LoadConfiguration(meta);
            config.MetadataMap.RemoveRecord(actorId);
            EditorUtility.SetDirty(config);
        }

        private static void MoveActorRecord (string sourceId, string destinationId, ActorMetadata meta)
        {
            var config = LoadConfiguration(meta);
            config.MetadataMap.MoveRecord(sourceId, destinationId);
            EditorUtility.SetDirty(config);
        }

        private static ActorManagerConfiguration LoadConfiguration (ActorMetadata meta)
        {
            var configType = ResolveConfigurationType(meta);
            var config = ProjectConfigurationProvider.LoadOrDefault(configType) as ActorManagerConfiguration;
            if (!config) throw new InvalidOperationException($"Failed to load `{configType.FullName}` configuration.");
            return config;
        }

        private static Type ResolveConfigurationType (ActorMetadata meta)
        {
            if (meta is CharacterMetadata) return typeof(CharactersConfiguration);
            else if (meta is BackgroundMetadata) return typeof(BackgroundsConfiguration);
            else if (meta is TextPrinterMetadata) return typeof(TextPrintersConfiguration);
            else if (meta is ChoiceHandlerMetadata) return typeof(ChoiceHandlersConfiguration);
            else throw new NotSupportedException($"Unknown metadata type: `{meta.GetType().FullName}`.");
        }

        private static void RemoveAssociatedResources (ActorMetadata metadata)
        {
            if (editorResources is null)
                editorResources = EditorResources.LoadOrDefault();
            var categoryId = metadata.GetResourceCategoryId();
            editorResources.RemoveCategory(categoryId);
            EditorUtility.SetDirty(editorResources);
        }
    }
}
