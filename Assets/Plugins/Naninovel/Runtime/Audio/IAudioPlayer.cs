// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to play <see cref="AudioClip"/>.
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// Current volume of the player.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Checks whether provided clip is currently playing.
        /// </summary>
        bool IsPlaying (AudioClip clip);
        /// <summary>
        /// Starts playback of the provided clip.
        /// </summary>
        void Play (AudioClip clip, AudioSource audioSource = null, float volume = 1f,
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false);
        /// <summary>
        /// Starts playback of the provided clip fading in volume over the provided time (in seconds).
        /// </summary>
        UniTask PlayAsync (AudioClip clip, float fadeInTime, AudioSource audioSource = null, float volume = 1f,
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false, 
            AsyncToken asyncToken = default);
        /// <summary>
        /// Stops playback of the provided clip.
        /// </summary>
        void Stop (AudioClip clip);
        /// <summary>
        /// Stops playback of all the playing clips.
        /// </summary>
        void StopAll ();
        /// <summary>
        /// Stops playback of the provided clip fading out volume over the provided time (in seconds).
        /// </summary>
        UniTask StopAsync (AudioClip clip, float fadeOutTime, AsyncToken asyncToken = default);
        /// <summary>
        /// Stops playback of all the playing clips fading out volume over the provided time (in seconds).
        /// </summary>
        UniTask StopAllAsync (float fadeOutTime, AsyncToken asyncToken = default);
        /// <summary>
        /// Returns <see cref="IAudioTrack"/> associated with the provided clip.
        /// </summary>
        IReadOnlyCollection<IAudioTrack> GetTracks (AudioClip clip);
    }
}
