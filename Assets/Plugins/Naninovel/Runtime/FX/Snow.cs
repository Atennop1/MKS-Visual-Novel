// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Commands;
using System.Linq;
using UnityEngine;

namespace Naninovel.FX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Snow : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable, DestroySpawned.IParameterized, DestroySpawned.IAwaitable
    {
        protected float Intensity { get; private set; }
        protected float FadeInTime { get; private set; }
        protected float FadeOutTime { get; private set; }

        [SerializeField] private float defaultIntensity = 100f;
        [SerializeField] private float defaultFadeInTime = 5f;
        [SerializeField] private float defaultFadeOutTime = 5f;

        private static readonly int tintColorId = Shader.PropertyToID("_TintColor");

        private readonly Tweener<FloatTween> intensityTweener = new Tweener<FloatTween>();
        private ParticleSystem particles;
        private ParticleSystem.EmissionModule emissionModule;
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
            var tween = new FloatTween(emissionModule.rateOverTimeMultiplier, Intensity, time, SetRateOverTime);
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
            var tween = new FloatTween(Intensity, 0, time, SetTintOpacity);
            await intensityTweener.RunAsync(tween, asyncToken, particles);
        }

        private void Awake ()
        {
            particles = GetComponent<ParticleSystem>();
            emissionModule = particles.emission;
            particlesMaterial = GetComponent<ParticleSystemRenderer>().material;
            tintColor = particlesMaterial.GetColor(tintColorId);
            
            // Position before the first background.
            transform.position = new Vector3(0, 0, Engine.GetConfiguration<BackgroundsConfiguration>().ZOffset - 1);

            SetRateOverTime(0);
        }

        private void SetRateOverTime (float value)
        {
            emissionModule.rateOverTimeMultiplier = value;
        }

        private void SetTintOpacity (float value)
        {
            var color = tintColor;
            color.a *= Mathf.Clamp01(value / defaultIntensity);
            particlesMaterial.SetColor(tintColorId, color);
        }
    }
}
