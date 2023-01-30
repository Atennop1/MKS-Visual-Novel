// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Plays an [SFX (sound effect)](/guide/audio.md#sound-effects) track with the provided name.
    /// Unlike [@sfx] command, the clip is played with minimum delay and is not serialized with the game state (won't be played after loading a game, even if it was played when saved).
    /// The command can be used to play various transient audio clips, such as UI-related sounds (eg, on button click with [`Play Script` component](/guide/user-interface.md#play-script-on-unity-event)).
    /// </summary>
    [CommandAlias("sfxFast")]
    public class PlaySfxFast : AudioCommand, Command.IPreloadable
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
        /// Whether to start playing the audio from start in case it's already playing.
        /// </summary>
        [ParameterDefaultValue("true")]
        public BooleanParameter Restart = true;
        /// <summary>
        /// Whether to allow playing multiple instances of the same clip; has no effect when `restart` is enabled.
        /// </summary>
        [ParameterDefaultValue("true")]
        public BooleanParameter Additive = true;
        /// <summary>
        /// Audio mixer [group path](https://docs.unity3d.com/ScriptReference/Audio.AudioMixer.FindMatchingGroups) that should be used when playing the audio.
        /// </summary>
        [ParameterAlias("group")]
        public StringParameter GroupPath;

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
            // Make sure the resource is loaded, it won't play otherwise.
            if (!AudioManager.AudioLoader.IsLoaded(SfxPath))
                await AudioManager.AudioLoader.LoadAsync(SfxPath);
            AudioManager.PlaySfxFast(SfxPath, Volume, GroupPath, Restart, Additive);   
        }
        
    } 
}
