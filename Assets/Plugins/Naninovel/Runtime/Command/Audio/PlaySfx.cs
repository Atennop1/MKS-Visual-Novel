// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel.Commands
{
    /// <summary>
    /// Plays or modifies currently played [SFX (sound effect)](/guide/audio.md#sound-effects) track with the provided name.
    /// </summary>
    /// <remarks>
    /// Sound effect tracks are not looped by default.
    /// When sfx track name (SfxPath) is not specified, will affect all the currently played tracks.
    /// When invoked for a track that is already playing, the playback won't be affected (track won't start playing from the start),
    /// but the specified parameters (volume and whether the track is looped) will be applied.
    /// </remarks>
    [CommandAlias("sfx")]
    public class PlaySfx : AudioCommand, Command.IPreloadable
    {
        /// <summary>
        /// Path to the sound effect asset to play.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ResourceContext(AudioConfiguration.DefaultAudioPathPrefix)]
        public StringParameter SfxPath;
        /// <summary>
        /// Volume of the sound effect.
        /// </summary>
        [ParameterDefaultValue("1")]
        public DecimalParameter Volume = 1f;
        /// <summary>
        /// Whether to play the sound effect in a loop.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Loop = false;
        /// <summary>
        /// Duration of the volume fade-in when starting playback, in seconds (0.0 by default); 
        /// doesn't have effect when modifying a playing track.
        /// </summary>
        [ParameterAlias("fade"), ParameterDefaultValue("0")]
        public DecimalParameter FadeInDuration = 0f;
        /// <summary>
        /// Audio mixer [group path](https://docs.unity3d.com/ScriptReference/Audio.AudioMixer.FindMatchingGroups) that should be used when playing the audio.
        /// </summary>
        [ParameterAlias("group")]
        public StringParameter GroupPath;
        /// <summary>
        /// Duration (in seconds) of the modification.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        public async UniTask PreloadResourcesAsync ()
        {
            if (!Assigned(SfxPath) || SfxPath.DynamicValue) return;
            await AudioManager.AudioLoader.LoadAndHoldAsync(SfxPath, this);
        }

        public void ReleasePreloadedResources ()
        {
            if (!Assigned(SfxPath) || SfxPath.DynamicValue) return;
            AudioManager?.AudioLoader?.Release(SfxPath, this);
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var duration = Assigned(Duration) ? Duration.Value : AudioManager.Configuration.DefaultFadeDuration;
            if (Assigned(SfxPath)) await PlayOrModifyTrackAsync(AudioManager, SfxPath, Volume, Loop, duration, FadeInDuration, GroupPath, asyncToken);
            else await UniTask.WhenAll(AudioManager.GetPlayedSfxPaths().ToList().Select(path => PlayOrModifyTrackAsync(AudioManager, path, Volume, Loop, duration, FadeInDuration, null, asyncToken)));
        }

        private static async UniTask PlayOrModifyTrackAsync (IAudioManager manager, string path, float volume, bool loop, float time, float fade, string group, AsyncToken asyncToken)
        {
            if (manager.IsSfxPlaying(path)) await manager.ModifySfxAsync(path, volume, loop, time, asyncToken);
            else await manager.PlaySfxAsync(path, volume, fade, loop, group, asyncToken);
        }
    }
}
