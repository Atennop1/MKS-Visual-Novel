// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ICameraManager"/>
    /// <remarks>Initialization order lowered, so the user could see something while waiting for the engine initialization.</remarks>
    [InitializeAtRuntime(-1)] 
    public class CameraManager : ICameraManager, IStatefulService<GameStateMap>, IStatefulService<SettingsStateMap>
    {
        [Serializable]
        public class Settings
        {
            public int QualityLevel = -1;
        }

        [Serializable]
        public class GameState
        {
            public Vector3 Offset = Vector3.zero;
            public Quaternion Rotation = Quaternion.identity;
            public float Zoom = 0f;
            public bool Orthographic = true;
            public CameraLookState LookMode = default;
            public CameraComponentState[] CameraComponents;
            public bool RenderUI = true;
        }

        public virtual CameraConfiguration Configuration { get; }
        public virtual Camera Camera { get; protected set; }
        public virtual Camera UICamera { get; protected set; }
        public virtual bool RenderUI
        {
            get => Configuration.UseUICamera ? UICamera.enabled : MaskUtils.GetLayer(Camera.cullingMask, uiLayer);
            set { if (Configuration.UseUICamera) UICamera.enabled = value; else Camera.cullingMask = MaskUtils.SetLayer(Camera.cullingMask, uiLayer, value); }
        }
        public virtual Vector3 Offset
        {
            get => offset;
            set { CompleteOffsetTween(); offset = value; ApplyOffset(value); }
        }
        public virtual Quaternion Rotation
        {
            get => rotation;
            set { CompleteRotationTween(); rotation = value; ApplyRotation(value); }
        }
        public virtual float Zoom
        {
            get => zoom;
            set { CompleteZoomTween(); zoom = value; ApplyZoom(value); }
        }
        public virtual bool Orthographic
        {
            get => Camera.orthographic;
            set { Camera.orthographic = value; Zoom = Zoom; }
        }
        public virtual int QualityLevel { get => QualitySettings.GetQualityLevel(); set => QualitySettings.SetQualityLevel(value, true); }

        protected virtual CameraLookController LookController { get; private set; }

        private readonly IInputManager inputManager;
        private readonly IEngineBehaviour engineBehaviour;
        private readonly RenderTexture thumbnailRenderTexture;
        private readonly List<MonoBehaviour> cameraComponentsCache = new List<MonoBehaviour>();
        private readonly Tweener<VectorTween> offsetTweener = new Tweener<VectorTween>();
        private readonly Tweener<VectorTween> rotationTweener = new Tweener<VectorTween>();
        private readonly Tweener<FloatTween> zoomTweener = new Tweener<FloatTween>();
        private IReadOnlyCollection<CameraComponentState> initialComponentState;
        private GameObject serviceObject;
        private Transform lookContainer;
        private Vector3 offset = Vector3.zero;
        private Quaternion rotation = Quaternion.identity;
        private float zoom = 0f;
        private float initialOrthoSize, initialFOV;
        private int uiLayer;

        public CameraManager (CameraConfiguration config, IInputManager inputManager, IEngineBehaviour engineBehaviour)
        {
            Configuration = config;
            this.inputManager = inputManager;
            this.engineBehaviour = engineBehaviour;
            
            thumbnailRenderTexture = new RenderTexture(config.ThumbnailResolution.x, config.ThumbnailResolution.y, 24);
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            uiLayer = Engine.GetConfiguration<UIConfiguration>().ObjectsLayer;
            serviceObject = Engine.CreateObject(nameof(CameraManager));
            lookContainer = Engine.CreateObject("MainCameraLookContainer", parent: serviceObject.transform).transform;
            lookContainer.position = Configuration.InitialPosition;
            Camera = InitializeMainCamera(Configuration, lookContainer, uiLayer);
            initialComponentState = GetComponentState(Camera);
            initialOrthoSize = Camera.orthographicSize;
            initialFOV = Camera.fieldOfView;
            if (Configuration.UseUICamera)
                UICamera = InitializeUICamera(Configuration, serviceObject.transform, uiLayer);
            LookController = new CameraLookController(Camera.transform, inputManager.GetCameraLookX(), inputManager.GetCameraLookY());
            engineBehaviour.OnBehaviourUpdate += LookController.Update;
            return UniTask.CompletedTask;
        }

        public virtual void ResetService ()
        {
            LookController.Enabled = false;
            Offset = Vector3.zero;
            Rotation = Quaternion.identity;
            Zoom = 0f;
            Orthographic = !Configuration.CustomCameraPrefab || Configuration.CustomCameraPrefab.orthographic;
            ApplyComponentState(Camera, initialComponentState);
        }

        public virtual void DestroyService ()
        {
            if (engineBehaviour != null)
                engineBehaviour.OnBehaviourUpdate -= LookController.Update;

            ObjectUtils.DestroyOrImmediate(thumbnailRenderTexture);
            ObjectUtils.DestroyOrImmediate(serviceObject);
        }

        public virtual void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                QualityLevel = QualityLevel
            };
            stateMap.SetState(settings);
        }

        public virtual UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.GetState<Settings>() ?? new Settings();
            if (settings.QualityLevel >= 0 && settings.QualityLevel != QualityLevel)
                QualityLevel = settings.QualityLevel;

            return UniTask.CompletedTask;
        }

        public virtual void SaveServiceState (GameStateMap stateMap)
        {
            var gameState = new GameState {
                Offset = Offset,
                Rotation = Rotation,
                Zoom = Zoom,
                Orthographic = Orthographic,
                LookMode = LookController.GetState(),
                RenderUI = RenderUI,
                CameraComponents = GetComponentState(Camera)
            };
            stateMap.SetState(gameState);
        }

        public virtual UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.GetState<GameState>();
            if (state is null)
            {
                ResetService();
                return UniTask.CompletedTask;
            }
            
            Offset = state.Offset;
            Rotation = state.Rotation;
            Zoom = state.Zoom;
            Orthographic = state.Orthographic;
            RenderUI = state.RenderUI;
            SetLookMode(state.LookMode.Enabled, state.LookMode.Zone, state.LookMode.Speed, state.LookMode.Gravity);
            ApplyComponentState(Camera, state.CameraComponents);
            return UniTask.CompletedTask;
        }

        public virtual void SetLookMode (bool enabled, Vector2 lookZone, Vector2 lookSpeed, bool gravity)
        {
            LookController.LookZone = lookZone;
            LookController.LookSpeed = lookSpeed;
            LookController.Gravity = gravity;
            LookController.Enabled = enabled;
        }

        public virtual Texture2D CaptureThumbnail ()
        {
            if (Configuration.HideUIInThumbnails)
                RenderUI = false;

            // Hide the save-load menu in case it's visible.
            var saveLoadUI = Engine.GetService<IUIManager>()?.GetUI<UI.ISaveLoadUI>();
            var saveLoadUIWasVisible = saveLoadUI?.Visible;
            if (saveLoadUIWasVisible.HasValue && saveLoadUIWasVisible.Value)
                saveLoadUI.Visible = false;

            // Confirmation UI may still be visible here (due to a fade-out time); force-hide it.
            var confirmUI = Engine.GetService<IUIManager>()?.GetUI<UI.IConfirmationUI>();
            var confirmUIWasVisible = confirmUI?.Visible ?? false;
            if (confirmUI != null) confirmUI.Visible = false;

            var initialRenderTexture = Camera.targetTexture;
            Camera.targetTexture = thumbnailRenderTexture;
            ForceTransitionalSpritesUpdate();
            Camera.Render();
            Camera.targetTexture = initialRenderTexture;

            if (RenderUI && Configuration.UseUICamera)
            {
                initialRenderTexture = UICamera.targetTexture;
                UICamera.targetTexture = thumbnailRenderTexture;
                UICamera.Render();
                UICamera.targetTexture = initialRenderTexture;
            }

            var thumbnail = thumbnailRenderTexture.ToTexture2D();

            // Restore the save-load menu and confirmation UI in case we hid them.
            if (saveLoadUIWasVisible.HasValue && saveLoadUIWasVisible.Value)
                saveLoadUI.Visible = true;
            if (confirmUIWasVisible)
                confirmUI.Visible = true;

            if (Configuration.HideUIInThumbnails)
                RenderUI = true;

            return thumbnail;

            void ForceTransitionalSpritesUpdate ()
            {
                var updateMethod = typeof(TransitionalSpriteRenderer).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateMethod is null) throw new Exception("Failed to locate `Update` method of transitional sprite renderer.");
                var sprites = UnityEngine.Object.FindObjectsOfType<TransitionalSpriteRenderer>();
                foreach (var sprite in sprites)
                    updateMethod.Invoke(sprite, null);
            }
        }

        public virtual async UniTask ChangeOffsetAsync (Vector3 offset, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            CompleteOffsetTween();

            if (duration > 0)
            {
                var currentOffset = this.offset;
                this.offset = offset;
                var tween = new VectorTween(currentOffset, offset, duration, ApplyOffset, false, easingType);
                await offsetTweener.RunAsync(tween, asyncToken, Camera);
            }
            else Offset = offset;
        }

        public virtual async UniTask ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            CompleteRotationTween();

            if (duration > 0)
            {
                var currentRotation = this.rotation;
                this.rotation = rotation;
                var tween = new VectorTween(currentRotation.ClampedEulerAngles(), rotation.ClampedEulerAngles(), duration, ApplyRotation, false, easingType);
                await rotationTweener.RunAsync(tween, asyncToken, Camera);
            }
            else Rotation = rotation;
        }

        public virtual async UniTask ChangeZoomAsync (float zoom, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            CompleteZoomTween();

            if (duration > 0)
            {
                var currentZoom = this.zoom;
                this.zoom = zoom;
                var tween = new FloatTween(currentZoom, zoom, duration, ApplyZoom, false, easingType);
                await zoomTweener.RunAsync(tween, asyncToken, Camera);
            }
            else Zoom = zoom;
        }

        protected virtual Camera InitializeMainCamera (CameraConfiguration config, Transform parent, int uiLayer)
        {
            if (config.CustomCameraPrefab != null)
            {
                var customCamera = Engine.Instantiate(config.CustomCameraPrefab, parent: parent);
                customCamera.transform.localPosition = Vector3.zero; // Position is controlled via look container.
                return customCamera;
            }

            var camera = Engine.CreateObject<Camera>("MainCamera", parent: parent);
            camera.transform.localPosition = Vector3.zero;
            camera.depth = 0;
            camera.backgroundColor = new Color32(35, 31, 32, 255);
            camera.orthographic = true;
            camera.orthographicSize = config.SceneRect.height / 2;
            camera.fieldOfView = 60f;
            if (!config.UseUICamera)
                camera.allowHDR = false; // Otherwise text artifacts appear when printing.
            if (Engine.Configuration.OverrideObjectsLayer) // When culling is enabled, render only the engine object and UI (when not using UI camera) layers.
                camera.cullingMask = config.UseUICamera ? 1 << Engine.Configuration.ObjectsLayer : (1 << Engine.Configuration.ObjectsLayer) | (1 << uiLayer);
            else if (config.UseUICamera) camera.cullingMask = ~(1 << uiLayer);
            return camera;
        }

        protected virtual Camera InitializeUICamera (CameraConfiguration config, Transform parent, int uiLayer)
        {
            if (config.CustomUICameraPrefab != null)
            {
                var customCamera = Engine.Instantiate(config.CustomUICameraPrefab, parent: parent);
                customCamera.transform.position = config.InitialPosition;
                return customCamera;
            }

            var camera = Engine.CreateObject<Camera>("UICamera", parent: parent);
            camera.depth = 1;
            camera.orthographic = true;
            camera.allowHDR = false; // Otherwise text artifacts appear when printing.
            camera.cullingMask = 1 << uiLayer;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.transform.position = config.InitialPosition;
            return camera;
        }

        protected virtual CameraComponentState[] GetComponentState (Camera camera)
        {
            camera.GetComponents(cameraComponentsCache);
            // Why zero? Camera is not a MonoBehaviour, so don't count it; others are considered custom effect.
            if (cameraComponentsCache.Count == 0) return Array.Empty<CameraComponentState>();
            return cameraComponentsCache.Select(c => new CameraComponentState(c)).ToArray();
        }
        
        protected virtual void ApplyComponentState (Camera camera, IReadOnlyCollection<CameraComponentState> state)
        {
            if (state is null) return;
            foreach (var compState in state)
                if (camera.GetComponent(compState.TypeName) is MonoBehaviour component)
                    component.enabled = compState.Enabled;
        }

        protected virtual void ApplyOffset (Vector3 offset)
        {
            lookContainer.position = Configuration.InitialPosition + offset;
        }

        protected virtual void ApplyRotation (Quaternion rotation)
        {
            lookContainer.rotation = rotation;
        }

        protected virtual void ApplyRotation (Vector3 rotation)
        {
            lookContainer.rotation = Quaternion.Euler(rotation);
        }

        protected virtual void ApplyZoom (float zoom)
        {
            if (Orthographic) Camera.orthographicSize = initialOrthoSize * (1f - Mathf.Clamp(zoom, 0, .99f));
            else Camera.fieldOfView = Mathf.Lerp(5f, initialFOV, 1f - zoom);
        }

        private void CompleteOffsetTween ()
        {
            if (offsetTweener.Running)
                offsetTweener.CompleteInstantly();
        }

        private void CompleteRotationTween ()
        {
            if (rotationTweener.Running)
                rotationTweener.CompleteInstantly();
        }

        private void CompleteZoomTween ()
        {
            if (zoomTweener.Running)
                zoomTweener.CompleteInstantly();
        }
    } 
}
