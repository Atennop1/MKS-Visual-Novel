// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Stops playing an SFX (sound effect) track with the provided name.
    /// </summary>
    /// <remarks>
    /// When sound effect track name (SfxPath) is not specified, will stop all the currently played tracks.
    /// </remarks>
    public class StopSfx : AudioCommand
    {
        /// <summary>
        /// Path to the sound effect to stop.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ResourceContext(AudioConfiguration.DefaultAudioPathPrefix)]
        public StringParameter SfxPath;
        /// <summary>
        /// Duration of the volume fade-out before stopping playback, in seconds (0.35 by default).
        /// </summary>
        [ParameterAlias("fade"), ParameterDefaultValue("0.35")]
        public DecimalParameter FadeOutDuration = .35f;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            if (Assigned(SfxPath)) await AudioManager.StopSfxAsync(SfxPath, FadeOutDuration, asyncToken);
            else await AudioManager.StopAllSfxAsync(FadeOutDuration, asyncToken);
        }
    } 
}
