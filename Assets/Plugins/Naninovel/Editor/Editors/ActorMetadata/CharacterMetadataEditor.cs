// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public class CharacterMetadataEditor : OrthoMetadataEditor<ICharacterActor, CharacterMetadata>
    {
        protected override Action<SerializedProperty> GetCustomDrawer (string propertyName)
        {
            switch (propertyName)
            {
                case nameof(CharacterMetadata.BakedLookDirection): return DrawWhen(HasResources);
                case nameof(CharacterMetadata.NameColor): return DrawWhen(Metadata.UseCharacterColor);
                case nameof(CharacterMetadata.MessageColor): return DrawWhen(Metadata.UseCharacterColor);
                case nameof(CharacterMetadata.HighlightWhenSpeaking): return DrawWhen(HasResources);
                case nameof(CharacterMetadata.HighlightCharacterCount): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.SpeakingPose): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.NotSpeakingPose): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.PlaceOnTop): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.HighlightDuration): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.HighlightEasing): return DrawWhen(HasResources && Metadata.HighlightWhenSpeaking);
                case nameof(CharacterMetadata.MessageSoundPlayback): return DrawWhen(!string.IsNullOrEmpty(Metadata.MessageSound));
                case nameof(CharacterMetadata.VoiceSource): return DrawWhen(HasResources);
                case nameof(CharacterMetadata.Poses): return DrawWhen(HasResources, ActorPosesEditor.Draw);
            }
            return base.GetCustomDrawer(propertyName);
        }
    }
}
