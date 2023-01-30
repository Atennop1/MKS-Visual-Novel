// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Naninovel.Runtime.UI;
using UnityEngine;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CGGalleryPanel : CustomUI, ICGGalleryUI
    {
        public const string CGPrefix = "CG";

        public virtual int CGCount { get; private set; }

        protected virtual ResourceLoaderConfiguration[] CGSources => cgSources;
        protected virtual CGViewerPanel ViewerPanel => viewerPanel;
        protected virtual CGGalleryGrid Grid => grid;

        [Tooltip("The specified resource loaders will be used to retrieve the available CG slots and associated textures.")]
        [SerializeField] private ResourceLoaderConfiguration[] cgSources = {
            new ResourceLoaderConfiguration { PathPrefix = $"{UnlockablesConfiguration.DefaultPathPrefix}/{CGPrefix}" },
            new ResourceLoaderConfiguration { PathPrefix = $"{BackgroundsConfiguration.DefaultPathPrefix}/{BackgroundsConfiguration.MainActorId}/{CGPrefix}" }
        };
        [Tooltip("Used to view selected CG slots.")]
        [SerializeField] private CGViewerPanel viewerPanel = default;
        [Tooltip("Used to host and navigate selectable CG preview thumbnails.")]
        [SerializeField] private CGGalleryGrid grid = default;

        private IResourceProviderManager providerManager;
        private ILocalizationManager localizationManager;
        private IInputManager inputManager;

        public override async UniTask InitializeAsync ()
        {
            var slotData = new List<CGSlotData>();
            await UniTask.WhenAll(CGSources.Select(InitializeLoaderAsync));
            CGCount = slotData.Count;
            Grid.Initialize(viewerPanel, slotData);

            async UniTask InitializeLoaderAsync (ResourceLoaderConfiguration loaderConfig)
            {
                var loader = loaderConfig.CreateLocalizableFor<Texture2D>(providerManager, localizationManager);
                var resourcePaths = await loader.LocateAsync(string.Empty);
                var pathsBySlots = resourcePaths.OrderBy(p => p).GroupBy(CGPathToSlotId);
                foreach (var pathsBySlot in pathsBySlots)
                    AddSlotData(pathsBySlot, loader);
            }

            string CGPathToSlotId (string cgPath)
            {
                if (cgPath.Contains(CGPrefix + "/"))
                    cgPath = cgPath.GetAfterFirst(CGPrefix + "/");
                if (!cgPath.Contains("_")) return cgPath;
                if (!ParseUtils.TryInvariantInt(cgPath.GetAfter("_"), out _)) return cgPath;
                return cgPath.GetBeforeLast("_");
            }

            void AddSlotData (IGrouping<string, string> pathsBySlot, IResourceLoader<Texture2D> loader)
            {
                var id = pathsBySlot.Key;
                if (slotData.Any(s => s.Id == id)) return;
                var data = new CGSlotData(id, pathsBySlot.OrderBy(p => p), loader);
                slotData.Add(data);
            }
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(Grid, ViewerPanel);

            providerManager = Engine.GetService<IResourceProviderManager>();
            localizationManager = Engine.GetService<ILocalizationManager>();
            inputManager = Engine.GetService<IInputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (inputManager?.GetCancel() != null)
                inputManager.GetCancel().OnStart += ViewerPanel.Hide;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (inputManager?.GetCancel() != null)
                inputManager.GetCancel().OnStart -= ViewerPanel.Hide;
        }
    }
}
