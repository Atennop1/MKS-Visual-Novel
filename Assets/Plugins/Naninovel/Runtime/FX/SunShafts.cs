// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Commands;
using System.Linq;
using UnityEngine;

namespace Naninovel.FX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SunShafts : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable, DestroySpawned.IParameterized, DestroySpawned.IAwaitable
    {
        protected float Intensity { get; private set; }
        protected float FadeInTime { get; private set; }
        protected float FadeOutTime { get; private set; }

        [SerializeField] private float defaultIntensity = .85f;
        [SerializeField] private float defaultFadeInTime = 3f;
        [SerializeField] private float defaultFadeOutTime = 3f;

        private static readonly int tintColorId = Shader.PropertyToID("_TintColor");

        private readonly Tweener<FloatTween> intensityTweener = new Tweener<FloatTween>();
        private ParticleSystem particles;
        private Material particlesMaterial;
        private Color tintColor;

        public virtual void SetSpawnParameters (IReadOnlyList<string> parameters)
        {
            Intensity = parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultIntensity;
            FadeInTime = Mathf.Abs(parameters?.ElementAtOrDefault(1)?.AsInvariantFloat() ?? defaultFadeInTime);
        }

        public async UniTask AwaitSpawnAsync (AsyncToken asyncToken = default)
        {
            if (intensityTweener.Running)
                intensityTweener.CompleteInstantly();

            var time = asyncToken.Completed ? 0 : FadeInTime;
            var tween = new FloatTween(0, Intensity, time, SetIntensity);
            await intensityTweener.RunAsync(tween, asyncToken, particles);
        }

        public void SetDestroyParameters (IReadOnlyList<string> parameters)
        {
            FadeOutTime = Mathf.Abs(parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultFadeOutTime);
        }

        public async UniTask AwaitDestroyAsync (AsyncToken asyncToken = default)
        {
            if (intensityTweener.Running)
                intensityTweener.CompleteInstantly();

            var time = asyncToken.Completed ? 0 : FadeOutTime;
            var tween = new FloatTween(Intensity, 0, time, SetIntensity);
            await intensityTweener.RunAsync(tween, asyncToken, particles);
        }

        private void Awake ()
        {
            particles = GetComponent<ParticleSystem>();
            particlesMaterial = GetComponent<ParticleSystemRenderer>().material;
            tintColor = particlesMaterial.GetColor(tintColorId);

            // Position before the first background.
            transform.position = new Vector3(0, 0, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset - 1);
            particles.Play(true); // Prevent prewarm particles from spawning before position change.
        }

        private void SetIntensity (float value)
        {
            var color = tintColor;
            color.a *= value;
            particlesMaterial.SetColor(tintColorId, color);
        }
    }
}
