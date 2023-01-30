// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="IUIManager"/>
    [InitializeAtRuntime]
    public class UIManager : IUIManager, IStatefulService<SettingsStateMap>
    {
        [Serializable]
        public class Settings
        {
            public string FontName = default;
            public int FontSize = -1;
        }

        public virtual UIConfiguration Configuration { get; }
        public virtual string FontName { get => fontName; set => SetFontName(value); }
        public virtual int FontSize { get => fontSize; set => SetFontSize(value); }

        private readonly List<ManagedUI> managedUIs = new List<ManagedUI>();
        private readonly Dictionary<Type, IManagedUI> cachedGetUIResults = new Dictionary<Type, IManagedUI>();
        private readonly Dictionary<IManagedUI, bool> modalState = new Dictionary<IManagedUI, bool>();
        private readonly ICameraManager cameraManager;
        private readonly IInputManager inputManager;
        private readonly IResourceProviderManager providersManager;
        private ResourceLoader<GameObject> loader;
        private IInputSampler toggleUIInput;
        private string fontName;
        private int fontSize = -1;

        public UIManager (UIConfiguration config, IResourceProviderManager providersManager, ICameraManager cameraManager, IInputManager inputManager)
        {
            Configuration = config;
            this.providersManager = providersManager;
            this.cameraManager = cameraManager;
            this.inputManager = inputManager;

            // Instantiating the UIs after the engine initialization so that UIs can use Engine API in Awake() and OnEnable() methods.
            Engine.AddPostInitializationTask(InstantiateUIsAsync);
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            loader = Configuration.Loader.CreateFor<GameObject>(providersManager);

            toggleUIInput = inputManager.GetToggleUI();
            if (toggleUIInput != null)
                toggleUIInput.OnStart += ToggleUI;

            return UniTask.CompletedTask;
        }

        public virtual void ResetService () { }

        public virtual void DestroyService ()
        {
            if (toggleUIInput != null)
                toggleUIInput.OnStart -= ToggleUI;

            foreach (var ui in managedUIs)
                ObjectUtils.DestroyOrImmediate(ui.GameObject);
            managedUIs.Clear();
            cachedGetUIResults.Clear();

            loader?.ReleaseAll(this);

            Engine.RemovePostInitializationTask(InstantiateUIsAsync);
        }

        public virtual void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                FontName = FontName,
                FontSize = FontSize
            };
            stateMap.SetState(settings);
        }

        public virtual UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.GetState<Settings>() ?? new Settings {
                FontName = Configuration.DefaultFont
            };
            FontName = settings.FontName;
            FontSize = settings.FontSize;

            return UniTask.CompletedTask;
        }

        public virtual async UniTask<IManagedUI> AddUIAsync (GameObject prefab, string name = default)
        {
            var uiComponent = InstantiatePrefab(prefab, name);
            await uiComponent.InitializeAsync();
            return uiComponent;
        }

        public virtual IReadOnlyCollection<IManagedUI> GetManagedUIs ()
        {
            return managedUIs.Select(u => u.UIComponent).ToArray();
        }

        public virtual T GetUI<T> () where T : class, IManagedUI => GetUI(typeof(T)) as T;

        public virtual IManagedUI GetUI (Type type)
        {
            if (cachedGetUIResults.TryGetValue(type, out var cachedResult))
                return cachedResult;

            foreach (var managedUI in managedUIs)
                if (type.IsAssignableFrom(managedUI.ComponentType))
                {
                    var result = managedUI.UIComponent;
                    cachedGetUIResults[type] = result;
                    return managedUI.UIComponent;
                }

            return null;
        }

        public virtual IManagedUI GetUI (string name)
        {
            foreach (var managedUI in managedUIs)
                if (managedUI.Name == name)
                    return managedUI.UIComponent;
            return null;
        }

        public virtual bool RemoveUI (IManagedUI managedUI)
        {
            if (!this.managedUIs.Any(u => u.UIComponent == managedUI))
                return false;

            var ui = this.managedUIs.FirstOrDefault(u => u.UIComponent == managedUI);
            this.managedUIs.Remove(ui);
            foreach (var kv in cachedGetUIResults.ToList())
            {
                if (kv.Value == managedUI)
                    cachedGetUIResults.Remove(kv.Key);
            }

            ObjectUtils.DestroyOrImmediate(ui.GameObject);

            return true;
        }

        public virtual void SetUIVisibleWithToggle (bool visible, bool allowToggle = true)
        {
            cameraManager.RenderUI = visible;

            var clickThroughPanel = GetUI<ClickThroughPanel>();
            if (clickThroughPanel)
            {
                if (visible) clickThroughPanel.Hide();
                else
                {
                    if (allowToggle) clickThroughPanel.Show(true, ToggleUI, InputConfiguration.SubmitName, InputConfiguration.ToggleUIName);
                    else clickThroughPanel.Show(false, null);
                }
            }
        }

        public virtual void SetModalUI (IManagedUI modalUI)
        {
            if (modalState.Count > 0) // Restore previous state.
            {
                foreach (var kv in modalState)
                    kv.Key.Interactable = kv.Value || (kv.Key is CustomUI customUI && customUI.ModalUI && customUI.Visible);
                modalState.Clear();
            }

            if (modalUI is null) return;

            foreach (var ui in managedUIs)
            {
                modalState[ui.UIComponent] = ui.UIComponent.Interactable;
                ui.UIComponent.Interactable = false;
            }

            modalUI.Interactable = true;
        }

        protected virtual IManagedUI InstantiatePrefab (GameObject prefab, string name = default)
        {
            var gameObject = Engine.Instantiate(prefab, prefab.name, Configuration.OverrideObjectsLayer ? (int?)Configuration.ObjectsLayer : null);

            if (!gameObject.TryGetComponent<IManagedUI>(out var uiComponent))
                throw new Exception($"Failed to instantiate `{prefab.name}` UI prefab: the prefab doesn't contain a `{nameof(CustomUI)}` or `{nameof(IManagedUI)}` component on the root object.");

            if (!uiComponent.RenderCamera)
                uiComponent.RenderCamera = cameraManager.UICamera ? cameraManager.UICamera : cameraManager.Camera;

            if (!string.IsNullOrEmpty(FontName) && Configuration.GetFontOption(FontName) is UIConfiguration.FontOption fontOption)
                uiComponent.SetFont(fontOption.Font, fontOption.TMPFont);
            if (FontSize >= 0)
                uiComponent.SetFontSize(FontSize);

            var managedUI = new ManagedUI(name ?? prefab.name, gameObject, uiComponent);
            this.managedUIs.Add(managedUI);

            return uiComponent;
        }

        protected virtual void SetFontName (string fontName)
        {
            if (FontName == fontName) return;

            this.fontName = fontName;

            if (string.IsNullOrEmpty(fontName))
            {
                foreach (var ui in managedUIs)
                    ui.UIComponent.SetFont(null, null);
                return;
            }

            var fontOption = Configuration.GetFontOption(fontName);
            if (fontOption is null) throw new Exception($"Failed to set `{fontName}` font: Font option with the name is not assigned in the UI configuration.");

            foreach (var ui in managedUIs)
                ui.UIComponent.SetFont(fontOption.Font, fontOption.TMPFont);
        }

        protected virtual void SetFontSize (int size)
        {
            if (fontSize == size) return;

            fontSize = size;

            foreach (var ui in managedUIs)
                ui.UIComponent.SetFontSize(size);
        }

        protected virtual void ToggleUI () => SetUIVisibleWithToggle(!cameraManager.RenderUI);

        protected virtual async UniTask InstantiateUIsAsync ()
        {
            var resources = await loader.LoadAndHoldAllAsync(this);
            foreach (var resource in resources)
                InstantiatePrefab(resource, loader.GetLocalPath(resource));
            var tasks = managedUIs.Select(u => u.UIComponent.InitializeAsync());
            await UniTask.WhenAll(tasks);
        }
    }
}
