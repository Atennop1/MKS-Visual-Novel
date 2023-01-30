// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Naninovel.Runtime.UI;
using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class CGGalleryGridSlot : ScriptableGridSlot
    {
        public override string Id => Data.Id;

        protected virtual CGSlotData Data { get; private set; }
        protected virtual RawImage ThumbnailImage => thumbnailImage;
        protected virtual Texture2D LockedTexture => lockedTexture;
        protected virtual Texture2D LoadingTexture => loadingTexture;
        protected virtual IReadOnlyList<Texture2D> CGTextures { get; private set; }
        protected virtual bool AnyUnlocked => CGTextures?.Any(t => t != null) ?? false;

        [SerializeField] private RawImage thumbnailImage = null;
        [SerializeField] private Texture2D lockedTexture = default;
        [SerializeField] private Texture2D loadingTexture = default;

        private IUnlockableManager unlockableManager;
        private ILocalizationManager localizationManager;
        private CGViewerPanel viewerPanel;

        public void Initialize (CGViewerPanel viewerPanel)
        {
            this.viewerPanel = viewerPanel;
        }

        public void Bind (CGSlotData data)
        {
            UnloadCGTextures();
            this.Data = data;
            Refresh();
        }

        protected virtual async UniTask LoadCGTexturesAsync ()
        {
            var prevThumbnailImage = ThumbnailImage.texture;
            ThumbnailImage.texture = LoadingTexture;
            var textures = new Texture2D[Data.TexturePaths.Count];
            await UniTask.WhenAll(Data.TexturePaths.Select(LoadCGTextureAsync));
            CGTextures = textures;
            ThumbnailImage.texture = prevThumbnailImage;

            async UniTask LoadCGTextureAsync (string path)
            {
                var unlockableId = PathToUnlockableId(path);
                if (!unlockableManager.ItemUnlocked(unlockableId)) return;
                var index = Data.TexturePaths.IndexOf(path);
                textures[index] = Data.TextureLoader.IsLoaded(path)
                    ? Data.TextureLoader.GetLoadedOrNull(path)
                    : await Data.TextureLoader.LoadAndHoldAsync(path, this);
            }
        }

        public virtual void UnloadCGTextures ()
        {
            if (Data.TexturePaths is null) return;
            foreach (var texturePath in Data.TexturePaths)
                Data.TextureLoader?.Release(texturePath, this);
        }

        protected virtual void Refresh () => HandleItemUpdated(null);

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(ThumbnailImage, LockedTexture);

            unlockableManager = Engine.GetService<IUnlockableManager>();
            localizationManager = Engine.GetService<ILocalizationManager>();
            ThumbnailImage.texture = LoadingTexture;

            unlockableManager.OnItemUpdated += HandleItemUpdated;
            localizationManager.OnLocaleChanged += HandleLocaleChanged;
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            if (unlockableManager != null)
                unlockableManager.OnItemUpdated -= HandleItemUpdated;
            if (localizationManager != null)
                localizationManager.OnLocaleChanged -= HandleLocaleChanged;
        }

        protected virtual async void HandleItemUpdated (UnlockableItemUpdatedArgs _)
        {
            while (Id is null) // We get here after first OnEnable, but ID is not set yet.
            {
                await UniTask.DelayFrame(1);
                if (!this) return;
            }

            await LoadCGTexturesAsync();

            if (!AnyUnlocked) ThumbnailImage.texture = LockedTexture;
            else ThumbnailImage.texture = CGTextures.FirstOrDefault(t => t != null);
        }

        protected virtual void HandleLocaleChanged (string _) => Refresh();

        protected override void OnButtonClick ()
        {
            base.OnButtonClick();

            if (AnyUnlocked)
                viewerPanel.Show(CGTextures);
        }

        private static string PathToUnlockableId (string path) => $"{CGGalleryPanel.CGPrefix}/{path}";
    }
}
