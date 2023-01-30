// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    [EditInProjectSettings]
    public class AudioConfiguration : Configuration
    {
        public const string DefaultAudioPathPrefix = "Audio";
        public const string DefaultVoicePathPrefix = "Voice";
        public const string AutoVoiceClipNameTemplate = "{0}/{1}.{2}";

        [Tooltip("Configuration of the resource loader used with audio (BGM and SFX) resources.")]
        public ResourceLoaderConfiguration AudioLoader = new ResourceLoaderConfiguration { PathPrefix = DefaultAudioPathPrefix };
        [Tooltip("Configuration of the resource loader used with voice resources.")]
        public ResourceLoaderConfiguration VoiceLoader = new ResourceLoaderConfiguration { PathPrefix = DefaultVoicePathPrefix };
        [Tooltip(nameof(IAudioPlayer) + " implementation responsible for playing audio clips.")]
        public string AudioPlayer = typeof(AudioPlayer).AssemblyQualifiedName;
        [Range(0f, 1f), Tooltip("Master volume to set when the game is first started.")]
        public float DefaultMasterVolume = 1f;
        [Range(0f, 1f), Tooltip("BGM volume to set when the game is first started.")]
        public float DefaultBgmVolume = 1f;
        [Range(0f, 1f), Tooltip("SFX volume to set when the game is first started.")]
        public float DefaultSfxVolume = 1f;
        [Range(0f, 1f), Tooltip("Voice volume to set when the game is first started.")]
        public float DefaultVoiceVolume = 1f;
        [Tooltip("When enabled, each `@print` command will attempt to play an associated voice clip.")]
        public bool EnableAutoVoicing = false;
        [Tooltip("When auto voicing is enabled, controls method to associate voice clips with @print commands:" +
            "\n • Playback Spot — Voice clips are associated by script name, line and inline indexes (playback spot) of the @print commands. Works best when voicing is added after the scenario scripts are finished. Removing, adding or re-ordering scenario script lines will break the associations." +
            "\n • Content Hash — Voice clips are associated manually via voice map utility by the printed text and author name. Works best when adding voicing before the scenario scripts are finished. Removing, adding or re-ordering scenario script lines won't break the associations. Modifying printed text in the scripts will break associations only with the modified commands.\n\nConsult voicing documentation for more information and examples.")]
        public AutoVoiceMode AutoVoiceMode = AutoVoiceMode.PlaybackSpot;
        [Tooltip("Dictates how to handle concurrent voices playback:" +
            "\n • Allow Overlap — Concurrent voices will be played without limitation." +
            "\n • Prevent Overlap — Prevent concurrent voices playback by stopping any played voice clip before playing a new one." +
            "\n • Prevent Character Overlap — Prevent concurrent voices playback per character; voices of different characters (auto voicing) and any number of [@voice] command are allowed to be played concurrently.")]
        public VoiceOverlapPolicy VoiceOverlapPolicy = VoiceOverlapPolicy.PreventOverlap;
        [Tooltip("Assign localization tags to allow selecting voice language in the game settings independently of the main localization. Doesn't work with `Content Hash` auto voice mode.")]
        public List<string> VoiceLocales = default;
        [Tooltip("Default duration of the volume fade in/out when starting or stopping playing audio.")]
        public float DefaultFadeDuration = .35f;

        [Header("Audio Mixer")]
        [Tooltip("Audio mixer to control audio groups. When not provided, will use a default one.")]
        public AudioMixer CustomAudioMixer = default;
        [Tooltip("Name of the mixer's handle (exposed parameter) to control master volume.")]
        public string MasterVolumeHandleName = "Master Volume";
        [Tooltip("Path of the mixer's group to control master volume.")]
        public string BgmGroupPath = "Master/BGM";
        [Tooltip("Name of the mixer's handle (exposed parameter) to control background music volume.")]
        public string BgmVolumeHandleName = "BGM Volume";
        [Tooltip("Path of the mixer's group to control background music volume.")]
        public string SfxGroupPath = "Master/SFX";
        [Tooltip("Name of the mixer's handle (exposed parameter) to control sound effects volume.")]
        public string SfxVolumeHandleName = "SFX Volume";
        [Tooltip("Path of the mixer's group to control sound effects volume.")]
        public string VoiceGroupPath = "Master/Voice";
        [Tooltip("Name of the mixer's handle (exposed parameter) to control voice volume.")]
        public string VoiceVolumeHandleName = "Voice Volume";

        /// <summary>
        /// Generates auto voice clip (local) resource path based on the provided playback spot.
        /// </summary>
        public static string GetAutoVoiceClipPath (PlaybackSpot playbackSpot)
        {
            return string.Format(AutoVoiceClipNameTemplate, playbackSpot.ScriptName, playbackSpot.LineNumber, playbackSpot.InlineIndex);
        }

        /// <summary>
        /// Generates auto voice clip (local) resource path based on the provided print command content (voice ID, author ID and printed text).
        /// </summary>
        public static string GetAutoVoiceClipPath (Commands.PrintText printCommand)
        {
            if (printCommand is null || !Command.Assigned(printCommand.Text)) return string.Empty;
            var text = printCommand.Text.DynamicValue ? printCommand.Text.DynamicValueText : printCommand.Text.Value;
            var content = $"{printCommand.AutoVoiceId}{printCommand.AuthorId}{text}";
            return CryptoUtils.PersistentHexCode(content);
        }
    }
}
