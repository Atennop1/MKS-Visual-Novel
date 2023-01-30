// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public class TextPrinterMetadataEditor : OrthoMetadataEditor<ITextPrinterActor, TextPrinterMetadata>
    {
        protected override Action<SerializedProperty> GetCustomDrawer (string propertyName)
        {
            switch (propertyName)
            {
                case nameof(TextPrinterMetadata.EnableDepthPass): return DrawNothing();
                case nameof(TextPrinterMetadata.DepthAlphaCutoff): return DrawNothing();
                case nameof(TextPrinterMetadata.CustomTextureShader): return DrawNothing();
                case nameof(TextPrinterMetadata.CustomSpriteShader): return DrawNothing();
                case nameof(TextPrinterMetadata.RenderTexture): return DrawNothing();
                case nameof(TextPrinterMetadata.RenderRectangle): return DrawNothing();
                case nameof(TextPrinterMetadata.SplitBacklogMessages): return DrawWhen(Metadata.AddToBacklog);
            }
            return base.GetCustomDrawer(propertyName);
        }
    }
}
