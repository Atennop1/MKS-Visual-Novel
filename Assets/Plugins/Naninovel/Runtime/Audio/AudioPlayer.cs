// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    public class AudioPlayer : IAudioPlayer, IDisposable
    {
        public float Volume { get => controller.Volume; set => controller.Volume = value; }

        private readonly AudioController controller;

        public AudioPlayer ()
        {
            controller = Engine.CreateObject<AudioController>();
        }

        public void Dispose ()
        {
            controller.StopAllClips();
            ObjectUtils.DestroyOrImmediate(controller.gameObject);
        }

        public bool IsPlaying (AudioClip clip) => controller.ClipPlaying(clip);

        public void Play (AudioClip clip, AudioSource audioSource = null, float volume = 1,
            bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false)
        {
            controller.PlayClip(clip, audioSource, volume, loop, mixerGroup, introClip, additive);
        }

        public UniTask PlayAsync (AudioClip clip, float fadeInTime, AudioSource audioSource = null,
            float volume = 1, bool loop = false, AudioMixerGroup mixerGroup = null, AudioClip introClip = null, bool additive = false, AsyncToken asyncToken = default)
        {
            return controller.PlayClipAsync(clip, fadeInTime, audioSource, volume, loop, mixerGroup, introClip, additive, asyncToken);
        }

        public void Stop (AudioClip clip) => controller.StopClip(clip);

        public void StopAll () => controller.StopAllClips();

        public UniTask StopAsync (AudioClip clip, float fadeOutTime, AsyncToken asyncToken = default)
        {
            return controller.StopClipAsync(clip, fadeOutTime, asyncToken);
        }

        public UniTask StopAllAsync (float fadeOutTime, AsyncToken asyncToken = default)
        {
            return controller.StopAllClipsAsync(fadeOutTime, asyncToken);
        }

        public IReadOnlyCollection<IAudioTrack> GetTracks (AudioClip clip) => controller.GetTracks(clip);
    }
}
