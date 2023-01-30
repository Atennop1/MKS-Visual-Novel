// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public class BackgroundMetadataEditor : OrthoMetadataEditor<IBackgroundActor, BackgroundMetadata>
    {
        protected override Action<SerializedProperty> GetCustomDrawer (string propertyName)
        {
            switch (propertyName)
            {
                case nameof(BackgroundMetadata.MatchMode): return DrawWhen(!IsGeneric);
                case nameof(BackgroundMetadata.CustomMatchRatio): return DrawWhen(!IsGeneric && Metadata.MatchMode == CameraMatchMode.Custom);
            }
            return base.GetCustomDrawer(propertyName);
        }
    }
}
