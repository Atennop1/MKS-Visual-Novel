// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public class OrthoMetadataEditor<TActor, TMeta> : MetadataEditor<TActor, TMeta>
        where TActor : IActor
        where TMeta : OrthoActorMetadata
    {
        protected override Action<SerializedProperty> GetCustomDrawer (string propertyName)
        {
            switch (propertyName)
            {
                case nameof(OrthoActorMetadata.Pivot): return DrawWhen(HasResources);
                case nameof(OrthoActorMetadata.PixelsPerUnit): return DrawWhen(HasResources);
                case nameof(OrthoActorMetadata.EnableDepthPass): return DrawWhen(HasResources);
                case nameof(OrthoActorMetadata.DepthAlphaCutoff): return DrawWhen(HasResources && Metadata.EnableDepthPass);
                case nameof(OrthoActorMetadata.CustomTextureShader): return DrawWhen(HasResources && !IsGeneric);
                case nameof(OrthoActorMetadata.CustomSpriteShader): return DrawWhen(HasResources && !IsGeneric && !Metadata.RenderTexture);
                case nameof(OrthoActorMetadata.RenderTexture): return DrawWhen(HasResources && !IsGeneric);
                case nameof(OrthoActorMetadata.RenderRectangle): return DrawWhen(!IsGeneric && Metadata.RenderTexture);
            }
            return base.GetCustomDrawer(propertyName);
        }
    }
}
