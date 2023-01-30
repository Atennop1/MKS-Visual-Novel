// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IAudioManager"/>.
    /// </summary>
    public static class AudioManagerExtensions
    {
        /// <summary>
        /// Checks whether a BGM track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        public static bool IsBgmPlaying (this IAudioManager manager, string path)
        {
            return manager.GetPlayedBgmPaths().Contains(path);
        }

        /// <summary>
        /// Checks whether an SFX track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the audio resource.</param>
        public static bool IsSfxPlaying (this IAudioManager manager, string path)
        {
            return manager.GetPlayedSfxPaths().Contains(path);
        }

        /// <summary>
        /// Checks whether a voice track with the provided resource path is currently playing.
        /// </summary>
        /// <param name="path">Name (local path) of the voice resource.</param>
        public static bool IsVoicePlaying (this IAudioManager manager, string path)
        {
            return manager.GetPlayedVoicePath() == path;
        }
    }
}
