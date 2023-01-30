// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="ICharacterActor"/>.
    /// </summary>
    [System.Serializable]
    public class CharacterMetadata : OrthoActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<CharacterMetadata> { }
        [System.Serializable]
        public class Pose : ActorPose<CharacterState> { }

        [Tooltip("Look direction as portrait (baked) on the character texture; required to properly flip characters to make them 'face' the correct side of the scene.")]
        public CharacterLookDirection BakedLookDirection = CharacterLookDirection.Left;
        [Tooltip("Full name of the character to display in printer name label UI. Will use character ID when not specified.\nIt's possible to localize the display names or bind them to a custom variable (and dynamically change throughout the game); see the guide on `Characters` -> `Display Names` for more info.")]
        public string DisplayName = default;
        [Tooltip("Whether to apply character-specific color to printer messages and name label UI.")]
        public bool UseCharacterColor = false;
        [Tooltip("Character-specific color to tint printer name label UI.")]
        public Color NameColor = Color.white;
        [Tooltip("Character-specific color to tint printer messages.")]
        public Color MessageColor = Color.white;
        [Tooltip("When enabled, will apply specified poses based on whether this actor is the author of the last printed text.")]
        public bool HighlightWhenSpeaking = false;
        [Tooltip("Highlight will happen only when the specified number (or more) of characters are visible on scene. Leave zero to highlight no matter the visible characters count.")]
        public int HighlightCharacterCount = 0;
        [Tooltip("Name of the pose to apply when the character is speaking. Leave empty to not apply any pose.")]
        public string SpeakingPose = default;
        [Tooltip("Name of the pose to apply when the character is not speaking. Leave empty to not apply any pose.")]
        public string NotSpeakingPose = default;
        [Tooltip("Whether to also move the highlighted character to the topmost position (closer to the camera over z-axis).")]
        public bool PlaceOnTop = true;
        [Tooltip("The highlight pose animation duration.")]
        public float HighlightDuration = .35f;
        [Tooltip("The highlight pose animation easing.")]
        public EasingType HighlightEasing = EasingType.SmoothStep;
        [Tooltip("Path to the sound (SFX) to play when printing (revealing) messages and the character is author. The sound will be played on each character reveal, so make sure it's very short and sharp (without any pause/silence at the beginning of the audio clip).")]
        [ResourcePopup(AudioConfiguration.DefaultAudioPathPrefix)]
        public string MessageSound = default;
        [Tooltip("When `Message Sound` is assigned, controls the playback type:" +
                 "\n • Looped — Play the sound in loop, stop when the message is fully revealed." +
                 "\n • One Shot — Play the sound from start for each revealed character in the message." +
                 "\n • One Shot Clipped — Same as `One Shot`, but restart the sound in case it's still playing while next character is revealed.")]
        public MessageSoundPlayback MessageSoundPlayback = default;
        [Tooltip("When assigned, will instantiate the game object under the character object and use the attached audio source component to play voice clips associated with the character.")]
        public AudioSource VoiceSource = default;
        [Tooltip("When the character is an author of a printed message, selected text printer will automatically be used to handle the printing. Only custom printers are allowed.")]
        [ActorPopup(TextPrintersConfiguration.DefaultPathPrefix)]
        public string LinkedPrinter = default;
        [Tooltip("Named states (poses) of the character; pose name can be used as appearance in `@char` commands to set enabled properties of the associated state.")]
        public List<Pose> Poses = new List<Pose>();

        public CharacterMetadata ()
        {
            Implementation = typeof(SpriteCharacter).AssemblyQualifiedName;
            Loader = new ResourceLoaderConfiguration { PathPrefix = CharactersConfiguration.DefaultPathPrefix };
            Pivot = new Vector2(.5f, .0f);
        }

        public override ActorPose<TState> GetPoseOrNull<TState> (string poseName) => Poses.FirstOrDefault(p => p.Name == poseName) as ActorPose<TState>;
    }
}
