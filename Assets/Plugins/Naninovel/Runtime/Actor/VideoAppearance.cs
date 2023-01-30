// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.Video;

namespace Naninovel
{
    public readonly struct VideoAppearance
    {
        public VideoPlayer Video { get; }
        public AudioSource Audio { get; }
        public GameObject GameObject => Video.gameObject;

        private readonly Tweener<FloatTween> volumeTweener;

        public VideoAppearance (VideoPlayer video, AudioSource audio)
        {
            Video = video;
            Audio = audio;
            volumeTweener = new Tweener<FloatTween>();
        }

        public async UniTask TweenVolumeAsync (float value, float time, AsyncToken asyncToken = default)
        {
            var source = Audio;
            var tween = new FloatTween(Audio.volume, value, time, v => source.volume = v);
            await volumeTweener.RunAsync(tween, asyncToken);
        }
    }
}
