// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Plays a voice clip at the provided path.
    /// </summary>
    [CommandAlias("voice")]
    public class PlayVoice : AudioCommand, Command.IPreloadable
    {
        /// <summary>
        /// Path to the voice clip to play.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ResourceContext(AudioConfiguration.DefaultVoicePathPrefix)]
        public StringParameter VoicePath;
        /// <summary>
        /// Volume of the playback.
        /// </summary>
        [ParameterDefaultValue("1")]
        public DecimalParameter Volume = 1f;
        /// <summary>
        /// Audio mixer [group path](https://docs.unity3d.com/ScriptReference/Audio.AudioMixer.FindMatchingGroups) that should be used when playing the audio.
        /// </summary>
        [ParameterAlias("group")]
        public StringParameter GroupPath;
        /// <summary>
        /// ID of the character actor this voice belongs to.
        /// When provided and [per-author volume](/guide/voicing.md#author-volume) is used, volume will be adjusted accordingly.
        /// </summary>
        public StringParameter AuthorId;

        public async UniTask PreloadResourcesAsync ()
        {
            if (!Assigned(VoicePath) || VoicePath.DynamicValue) return;
            await AudioManager.VoiceLoader.LoadAndHoldAsync(VoicePath, this);
        }

        public void ReleasePreloadedResources ()
        {
            if (!Assigned(VoicePath) || VoicePath.DynamicValue) return;
            AudioManager?.VoiceLoader?.Release(VoicePath, this);
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            await AudioManager.PlayVoiceAsync(VoicePath, Volume, GroupPath, AuthorId);
        }
    }
}
