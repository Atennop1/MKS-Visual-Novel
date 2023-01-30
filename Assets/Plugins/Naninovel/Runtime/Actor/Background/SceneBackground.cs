// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="Scene"/> to represent the actor.
    /// </summary>
    /// <remarks>
    /// The implementation requires scenes to be at `./Assets/Scenes` project folder; resource providers are not supported.
    /// Scenes should be added to the build settings.
    /// </remarks>
    #if UNITY_EDITOR
    [ActorResources(typeof(UnityEditor.SceneAsset), true)]
    #endif
    public class SceneBackground : MonoBehaviourActor<BackgroundMetadata>, IBackgroundActor
    {
        private class SceneData
        {
            public Scene Scene;
            public RenderTexture RenderTexture;
            public Camera Camera;
            public readonly HashSet<object> Holders = new HashSet<object>();
            public readonly HashSet<SceneBackground> ActorHolders = new HashSet<SceneBackground>();
        }

        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool Visible { get => visible; set => SetVisibility(value); }

        protected virtual TransitionalRenderer TransitionalRenderer { get; private set; }

        private static readonly Dictionary<string, SceneData> loadedScenes = new Dictionary<string, SceneData>();

        private BackgroundMatcher matcher;
        private string appearance;
        private bool visible;

        public SceneBackground (string id, BackgroundMetadata metadata)
            : base(id, metadata) { }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            TransitionalRenderer = TransitionalRenderer.CreateFor(ActorMetadata, GameObject, false);
            matcher = BackgroundMatcher.CreateFor(ActorMetadata, TransitionalRenderer);

            SetVisibility(false);
        }

        public override async UniTask ChangeAppearanceAsync (string appearance, float duration,
            EasingType easingType = default, Transition? transition = default, AsyncToken asyncToken = default)
        {
            this.appearance = appearance;
            if (string.IsNullOrEmpty(appearance)) return;
            var sceneData = await GetOrLoadSceneAsync(appearance);
            asyncToken.ThrowIfCanceled();
            sceneData.ActorHolders.Add(this);
            await TransitionalRenderer.TransitionToAsync(sceneData.RenderTexture, duration, easingType, transition, asyncToken);
        }

        public override async UniTask ChangeVisibilityAsync (bool visible, float duration,
            EasingType easingType = default, AsyncToken asyncToken = default)
        {
            this.visible = visible;
            await TransitionalRenderer.FadeToAsync(visible ? 1 : 0, duration, easingType, asyncToken);
        }

        public override async UniTask HoldResourcesAsync (string appearance, object holder)
        {
            if (string.IsNullOrEmpty(appearance)) return;
            var data = await GetOrLoadSceneAsync(appearance);
            data.Holders.Add(holder);
        }

        public override void ReleaseResources (string appearance, object holder)
        {
            if (string.IsNullOrEmpty(appearance)) return;
            if (!loadedScenes.TryGetValue(appearance, out var data)) return;
            data.Holders.Remove(holder);
            if (data.Holders.Count == 0) UnloadScene(appearance);
        }

        public override void Dispose ()
        {
            base.Dispose();
            matcher?.Stop();
            foreach (var appearance in loadedScenes.Keys.ToArray())
                UnloadUnused(appearance);

            void UnloadUnused (string appearance)
            {
                var data = loadedScenes[appearance];
                data.ActorHolders.Remove(this);
                if (data.ActorHolders.Count == 0) UnloadScene(appearance);
            }
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).Forget();

        protected virtual void SetVisibility (bool visible) => ChangeVisibilityAsync(visible, 0).Forget();

        protected override Color GetBehaviourTintColor () => TransitionalRenderer.TintColor;

        protected override void SetBehaviourTintColor (Color tintColor)
        {
            if (!Visible) tintColor.a = TransitionalRenderer.TintColor.a;
            TransitionalRenderer.TintColor = tintColor;
        }

        private static async UniTask<SceneData> GetOrLoadSceneAsync (string appearance)
        {
            if (loadedScenes.ContainsKey(appearance)) return loadedScenes[appearance];
            var renderTexture = CreateRenderTexture();
            var scene = await LoadSceneAsync(appearance);
            var camera = FindCameraInScene(scene);
            camera.targetTexture = renderTexture;
            var sceneData = new SceneData { Scene = scene, RenderTexture = renderTexture, Camera = camera };
            loadedScenes[appearance] = sceneData;
            return sceneData;
        }

        private static RenderTexture CreateRenderTexture ()
        {
            var resolution = Engine.GetConfiguration<CameraConfiguration>().ReferenceResolution;
            var descriptor = new RenderTextureDescriptor(resolution.x, resolution.y);
            return new RenderTexture(descriptor);
        }

        private static async UniTask<Scene> LoadSceneAsync (string appearance)
        {
            const string sceneRoot = "Assets/Scenes";
            var scenePath = $"{sceneRoot}/{appearance}.unity";
            await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded) throw new Exception($"Failed loading scene `{scenePath}`. Make sure the scene is added to the build settings and located at `{sceneRoot}`.");
            return scene;
        }

        private static Camera FindCameraInScene (Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            var camera = default(Camera);
            for (int i = 0; i < rootObjects.Length; i++)
                if (rootObjects[i].TryGetComponent<Camera>(out camera))
                    break;
            if (!camera) throw new Exception($"Camera component is not found in `{scene.path}` scene.");
            return camera;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData () => loadedScenes.Clear();

        private void UnloadScene (string appearance)
        {
            if (!loadedScenes.TryGetValue(appearance, out var data)) return;
            loadedScenes.Remove(appearance);
            if (data.Camera) data.Camera.targetTexture = null;
            ObjectUtils.DestroyOrImmediate(data.RenderTexture);
            SceneManager.UnloadSceneAsync(data.Scene);
        }
    }
}
