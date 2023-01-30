// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Commands;
using System.Linq;
using UnityEngine;

namespace Naninovel.FX
{
    public class DigitalGlitch : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable
    {
        protected float Duration { get; private set; }
        protected float Intensity { get; private set; }

        protected ISpawnManager SpawnManager => Engine.GetService<ISpawnManager>();
        protected virtual string SpawnedPath { get; private set; }

        [SerializeField] private Shader glitchShader = default;
        [SerializeField] private Texture glitchTexture = default;
        [SerializeField] private float defaultDuration = 1f;
        [SerializeField] private float defaultIntensity = 1f;
        
        private static readonly int glitchTex = Shader.PropertyToID("_GlitchTex");
        private static readonly int intensity = Shader.PropertyToID("_Intensity");
        private static readonly int colorTint = Shader.PropertyToID("_ColorTint");
        private static readonly int burnColors = Shader.PropertyToID("_BurnColors");
        private static readonly int dodgeColors = Shader.PropertyToID("_DodgeColors");
        private static readonly int performUVShifting = Shader.PropertyToID("_PerformUVShifting");
        private static readonly int performColorShifting = Shader.PropertyToID("_PerformColorShifting");
        private static readonly int performScreenShifting = Shader.PropertyToID("_PerformScreenShifting");
        private static readonly int filterRadius = Shader.PropertyToID("filterRadius");
        private static readonly int flipUp = Shader.PropertyToID("flipUp");
        private static readonly int flipDown = Shader.PropertyToID("flipDown");
        private static readonly int displace = Shader.PropertyToID("displace");
        
        private CameraComponent cameraComponent;

        public virtual void SetSpawnParameters (IReadOnlyList<string> parameters)
        {
            this.AssertRequiredObjects(glitchShader, glitchTexture);

            SpawnedPath = gameObject.name;

            Duration = Mathf.Abs(parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultDuration);
            Intensity = Mathf.Abs(parameters?.ElementAtOrDefault(1)?.AsInvariantFloat() ?? defaultIntensity);
        }

        public async UniTask AwaitSpawnAsync (AsyncToken asyncToken = default) 
        {
            if (cameraComponent is null)
            {
                var cameraManager = Engine.GetService<ICameraManager>().Camera;
                cameraComponent = cameraManager.gameObject.AddComponent<CameraComponent>();
                cameraComponent.Shader = glitchShader;
                cameraComponent.GlitchTexture = glitchTexture;
            }
            cameraComponent.Intensity = Intensity;

            await UniTask.Delay(System.TimeSpan.FromSeconds(Duration));
            asyncToken.ThrowIfCanceled();

            if (SpawnManager.IsSpawned(SpawnedPath))
                SpawnManager.DestroySpawned(SpawnedPath);
        }

        private void OnDestroy ()
        {
            if (cameraComponent)
                Destroy(cameraComponent);
        }

        private class CameraComponent : MonoBehaviour
        {
            public Shader Shader;
            public Texture GlitchTexture;
            public float Intensity = 1.0f;
            public bool RandomGlitchFrequency = true;
            public float GlitchFrequency = .5f;
            public bool PerformUVShifting = true;
            public float ShiftAmount = 1f;
            public bool PerformScreenShifting = true;
            public bool PerformColorShifting = true;
            public Color TintColor = new Color(.2f, .2f, 0f, 0f);
            public bool BurnColors = true;
            public bool DodgeColors = false;

            private Material Material
            {
                get
                {
                    if (material is null)
                    {
                        material = new Material(Shader);
                        material.SetTexture(glitchTex, GlitchTexture);
                        material.SetTextureScale(glitchTex, new Vector2(Screen.width / (float)GlitchTexture.width, Screen.height / (float)GlitchTexture.height));
                        material.hideFlags = HideFlags.HideAndDontSave;
                    }
                    return material;
                }
            }

            private float glitchUp, glitchDown, flicker, glitchUpTime = .05f, glitchDownTime = .05f, flickerTime = .5f;

            private Material material;

            private void Start ()
            {
                material = null; // force to reinit the material on scene start

                if (!Shader || !Shader.isSupported)
                    enabled = false;

                flickerTime = RandomGlitchFrequency ? Random.value : 1f - GlitchFrequency;
                glitchUpTime = RandomGlitchFrequency ? Random.value : .1f - GlitchFrequency / 10f;
                glitchDownTime = RandomGlitchFrequency ? Random.value : .1f - GlitchFrequency / 10f;
            }

            private void OnDisable ()
            {
                if (material) Destroy(material);
            }

            private void OnRenderImage (RenderTexture source, RenderTexture destination)
            {
                Material.SetFloat(intensity, Intensity);
                Material.SetColor(colorTint, TintColor);
                Material.SetFloat(burnColors, BurnColors ? 1 : 0);
                Material.SetFloat(dodgeColors, DodgeColors ? 1 : 0);
                Material.SetFloat(performUVShifting, PerformUVShifting ? 1 : 0);
                Material.SetFloat(performColorShifting, PerformColorShifting ? 1 : 0);
                Material.SetFloat(performScreenShifting, PerformScreenShifting ? 1 : 0);

                if (Intensity == 0) Material.SetFloat(filterRadius, 0);

                glitchUp += Time.deltaTime * Intensity;
                glitchDown += Time.deltaTime * Intensity;
                flicker += Time.deltaTime * Intensity;

                if (flicker > flickerTime)
                {
                    Material.SetFloat(filterRadius, Random.Range(-3f, 3f) * Intensity * ShiftAmount);
                    Material.SetTextureOffset(glitchTex, new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
                    flicker = 0;
                    flickerTime = RandomGlitchFrequency ? Random.value : 1f - GlitchFrequency;
                }

                if (glitchUp > glitchUpTime)
                {
                    if (Random.Range(0f, 1f) < .1f * Intensity) Material.SetFloat(flipUp, Random.Range(0f, 1f) * Intensity);
                    else Material.SetFloat(flipUp, 0);

                    glitchUp = 0;
                    glitchUpTime = RandomGlitchFrequency ? Random.value / 10f : .1f - GlitchFrequency / 10f;
                }

                if (glitchDown > glitchDownTime)
                {
                    if (Random.Range(0f, 1f) < .1f * Intensity) Material.SetFloat(flipDown, 1f - Random.Range(0f, 1f) * Intensity);
                    else Material.SetFloat(flipDown, 1f);

                    glitchDown = 0;
                    glitchDownTime = RandomGlitchFrequency ? Random.value / 10f : .1f - GlitchFrequency / 10f;
                }

                if (Random.Range(0f, 1f) < .1f * Intensity * (RandomGlitchFrequency ? 1 : GlitchFrequency))
                    Material.SetFloat(displace, Random.value * Intensity);
                else Material.SetFloat(displace, 0);

                Graphics.Blit(source, destination, Material);
            }
        }
    }
}
