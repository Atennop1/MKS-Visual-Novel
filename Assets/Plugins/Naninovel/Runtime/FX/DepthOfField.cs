// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Commands;
using System.Linq;
using UnityEngine;

namespace Naninovel.FX
{
    public class DepthOfField : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable, DestroySpawned.IParameterized, DestroySpawned.IAwaitable
    {
        protected float FocusDistance { get; private set; }
        protected float FocalLength { get; private set; }
        protected float Duration { get; private set; }
        protected float StopDuration { get; private set; }

        [SerializeField] private float defaultFocusDistance = 10f;
        [SerializeField] private float defaultFocalLength = 3.75f;
        [SerializeField] private float defaultDuration = 1f;

        private static readonly int blurTexId = Shader.PropertyToID("_BlurTex");
        private static readonly int distanceId = Shader.PropertyToID("_Distance");
        private static readonly int lensCoeffId = Shader.PropertyToID("_LensCoeff");
        private static readonly int maxCoCId = Shader.PropertyToID("_MaxCoC");
        private static readonly int rcpMaxCoCId = Shader.PropertyToID("_RcpMaxCoC");
        private static readonly int rcpAspectId = Shader.PropertyToID("_RcpAspect");
        
        private readonly Tweener<FloatTween> focusDistanceTweener = new Tweener<FloatTween>();
        private readonly Tweener<FloatTween> focalLengthTweener = new Tweener<FloatTween>();
        private CameraComponent cameraComponent;

        public virtual void SetSpawnParameters (IReadOnlyList<string> parameters)
        {
            if (cameraComponent is null)
            {
                var cameraManager = Engine.GetService<ICameraManager>().Camera;
                cameraComponent = cameraManager.gameObject.AddComponent<CameraComponent>();
                cameraComponent.UseCameraFov = false;
            }

            if (cameraComponent.PointOfFocus != null)
            {
                cameraComponent.FocusDistance = Vector3.Dot(cameraComponent.PointOfFocus.position - cameraComponent.transform.position, cameraComponent.transform.forward);
                cameraComponent.PointOfFocus = null;
            }

            var focusObjectName = parameters?.ElementAtOrDefault(0);
            if (string.IsNullOrEmpty(focusObjectName))
                FocusDistance = Mathf.Max(0.01f, parameters?.ElementAtOrDefault(1)?.AsInvariantFloat() ?? defaultFocusDistance);
            else
            {
                var obj = GameObject.Find(focusObjectName);
                if (ObjectUtils.IsValid(obj))
                    cameraComponent.PointOfFocus = obj.transform;
                else
                {
                    Debug.LogWarning($"Failed to find game object with name `{focusObjectName}`; depth of field effect will use a default focus distance.");
                    FocusDistance = defaultFocusDistance;
                }
            }
            FocalLength = Mathf.Abs(parameters?.ElementAtOrDefault(2)?.AsInvariantFloat() ?? defaultFocalLength);
            Duration = Mathf.Abs(parameters?.ElementAtOrDefault(3)?.AsInvariantFloat() ?? defaultDuration);
        }

        public async UniTask AwaitSpawnAsync (AsyncToken asyncToken = default) 
        {
            if (focusDistanceTweener.Running)
                focusDistanceTweener.CompleteInstantly();
            if (focalLengthTweener.Running)
                focalLengthTweener.CompleteInstantly();

            var duration = asyncToken.Completed ? 0 : Duration;
            var focusDistanceTween = new FloatTween(cameraComponent.FocusDistance, FocusDistance, duration, ApplyFocusDistance);
            var focalLengthTween = new FloatTween(cameraComponent.FocalLength, FocalLength, duration, ApplyFocalLength);

            await UniTask.WhenAll(focusDistanceTweener.RunAsync(focusDistanceTween, asyncToken, cameraComponent), 
                focalLengthTweener.RunAsync(focalLengthTween, asyncToken, cameraComponent));
        }

        public void SetDestroyParameters (IReadOnlyList<string> parameters)
        {
            StopDuration = Mathf.Abs(parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultDuration);
        }

        public async UniTask AwaitDestroyAsync (AsyncToken asyncToken = default)
        {
            if (focusDistanceTweener.Running)
                focusDistanceTweener.CompleteInstantly();
            if (focalLengthTweener.Running)
                focalLengthTweener.CompleteInstantly();

            var duration = asyncToken.Completed ? 0 : StopDuration;
            var focalLengthTween = new FloatTween(cameraComponent.FocalLength, 0, duration, ApplyFocalLength);
            await focalLengthTweener.RunAsync(focalLengthTween, asyncToken);
        }

        private void ApplyFocusDistance (float value)
        {
            cameraComponent.FocusDistance = value;
        }

        private void ApplyFocalLength (float value)
        {
            cameraComponent.FocalLength = value;
        }

        private void OnDestroy () // Required to disable the effect on rollback.
        {
            if (cameraComponent)
                Destroy(cameraComponent);
        }

        private class CameraComponent : MonoBehaviour
        {
            private enum KernelSizeType { Small, Medium, Large, VeryLarge }
            
            public Transform PointOfFocus { get; set; } = null;
            public float FocusDistance { get; set; } = 0f;
            public bool UseCameraFov { get; set; } = true;
            public float FocalLength { get; set; } = 0f;

            private const KernelSizeType kernelSize = KernelSizeType.Medium;
            private const float fNumber = 1.4f;
            private const float filmHeight = 0.024f;

            private Camera targetCamera;
            private Material material;

            private void OnEnable ()
            {
                targetCamera = GetComponent<Camera>();

                var shader = Shader.Find("Naninovel/FX/DepthOfField");
                if (!shader.isSupported || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                {
                    Debug.LogWarning("Naninovel's Depth Of Field is not supported on the current platform.");
                    return;
                }

                if (material is null)
                {
                    material = new Material(shader);
                    material.hideFlags = HideFlags.HideAndDontSave;
                }

                targetCamera.depthTextureMode |= DepthTextureMode.Depth;
            }

            private void OnDestroy ()
            {
                ObjectUtils.DestroyOrImmediate(material);
            }

            private void OnRenderImage (RenderTexture source, RenderTexture destination)
            {
                // If the material hasn't been initialized because of system
                // incompatibility, just blit and return.
                if (material == null)
                {
                    Graphics.Blit(source, destination);
                    // Try to disable itself if it's Player.
                    if (Application.isPlaying) enabled = false;
                    return;
                }

                var width = source.width;
                var height = source.height;

                SetUpShaderParameters(source);

                // Pass #1 - Downsampling, prefiltering and CoC calculation
                var rt1 = RenderTexture.GetTemporary(width / 2, height / 2, 0, RenderTextureFormat.ARGBHalf);
                source.filterMode = FilterMode.Point;
                Graphics.Blit(source, rt1, material, 0);

                // Pass #2 - Bokeh simulation
                var rt2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, RenderTextureFormat.ARGBHalf);
                rt1.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt1, rt2, material, 1 + (int)kernelSize);

                // Pass #3 - Additional blur
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt2, rt1, material, 5);

                // Pass #4 - Upsampling and composition
                material.SetTexture(blurTexId, rt1);
                Graphics.Blit(source, destination, material, 6);

                RenderTexture.ReleaseTemporary(rt1);
                RenderTexture.ReleaseTemporary(rt2);
            }

            private float CalculateFocalLength ()
            {
                if (!UseCameraFov) return FocalLength;
                var fov = targetCamera.fieldOfView * Mathf.Deg2Rad;
                return 0.5f * filmHeight / Mathf.Tan(0.5f * fov);
            }

            private float CalculateMaxCoCRadius (int screenHeight)
            {
                // Estimate the allowable maximum radius of CoC from the kernel
                // size (the equation below was empirically derived).
                const float radiusInPixels = (float)kernelSize * 4 + 6;

                // Applying a 5% limit to the CoC radius to keep the size of
                // TileMax/NeighborMax small enough.
                return Mathf.Min(0.05f, radiusInPixels / screenHeight);
            }

            private void SetUpShaderParameters (RenderTexture source)
            {
                var dist = PointOfFocus != null ? Vector3.Dot(PointOfFocus.position - targetCamera.transform.position, targetCamera.transform.forward) : FocusDistance;
                var f = CalculateFocalLength();
                var s1 = Mathf.Max(dist, f);
                material.SetFloat(distanceId, s1);

                var coeff = f * f / (fNumber * (s1 - f) * filmHeight * 2);
                coeff = Mathf.Max(.001f, coeff); // Clamp the value to prevent glitches when gradually disabling the effect.
                material.SetFloat(lensCoeffId, coeff);

                var maxCoC = CalculateMaxCoCRadius(source.height);
                material.SetFloat(maxCoCId, maxCoC);
                material.SetFloat(rcpMaxCoCId, 1 / maxCoC);

                var rcpAspect = (float)source.height / source.width;
                material.SetFloat(rcpAspectId, rcpAspect);
            }
        }
    }
}
