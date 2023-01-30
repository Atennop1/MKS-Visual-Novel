// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class SceneTransitionUI : CustomUI, ISceneTransitionUI
    {
        protected RawImage Image => image;

        [SerializeField] private RawImage image = default;

        private readonly Tweener<FloatTween> transitionTweener = new Tweener<FloatTween>();
        private ICameraManager cameraManager;
        private TransitionalMaterial material;
        private RenderTexture sceneTexture;

        public virtual async UniTask CaptureSceneAsync ()
        {
            if (sceneTexture)
                RenderTexture.ReleaseTemporary(sceneTexture);

            sceneTexture = RenderTexture.GetTemporary(cameraManager.Camera.scaledPixelWidth, cameraManager.Camera.scaledPixelHeight);
            var initialRenderTexture = cameraManager.Camera.targetTexture;
            cameraManager.Camera.targetTexture = sceneTexture;
            await UniTask.DelayFrame(1); // Camera.Render() not working in builds.
            cameraManager.Camera.targetTexture = initialRenderTexture;

            material.TransitionProgress = 0;
            material.MainTexture = sceneTexture;
            image.texture = sceneTexture;
            SetVisibility(true);
        }

        public virtual async UniTask TransitionAsync (Transition transition, float duration, EasingType easingType = EasingType.Linear, AsyncToken asyncToken = default)
        {
            if (transitionTweener.Running)
                transitionTweener.CompleteInstantly();

            material.UpdateRandomSeed();
            material.TransitionProgress = 0;
            material.TransitionName = transition.Name;
            material.TransitionParams = transition.Parameters;
            if (transition.DissolveTexture)
                material.DissolveTexture = transition.DissolveTexture;

            var transitionTexture = RenderTexture.GetTemporary(cameraManager.Camera.scaledPixelWidth, cameraManager.Camera.scaledPixelHeight);
            var initialRenderTexture = cameraManager.Camera.targetTexture;
            cameraManager.Camera.targetTexture = transitionTexture;
            material.TransitionTexture = transitionTexture;

            var tween = new FloatTween(material.TransitionProgress, 1, duration, value => material.TransitionProgress = value, false, easingType);
            try { await transitionTweener.RunAsync(tween, asyncToken, material); }
            catch (AsyncOperationCanceledException)
            {
                // Try restore camera target texture before cancellation, otherwise it'll mess when rolling back.
                if (cameraManager != null && cameraManager.Camera)
                    cameraManager.Camera.targetTexture = initialRenderTexture;
                RenderTexture.ReleaseTemporary(transitionTexture);
                throw new AsyncOperationCanceledException(asyncToken);
            }

            cameraManager.Camera.targetTexture = initialRenderTexture;
            SetVisibility(false);
            RenderTexture.ReleaseTemporary(transitionTexture);

            // In case of rollbacks, revert to the original scene texture.
            material.TransitionProgress = 0;
        }

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(image);

            cameraManager = Engine.GetService<ICameraManager>();
            material = new TransitionalMaterial(true);
            image.material = material;
        }

        protected override void OnDestroy ()
        {
            if (sceneTexture)
                RenderTexture.ReleaseTemporary(sceneTexture);
            ObjectUtils.DestroyOrImmediate(material);

            base.OnDestroy();
        }
    }
}
