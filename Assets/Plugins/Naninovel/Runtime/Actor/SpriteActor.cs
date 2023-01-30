// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using Naninovel.FX;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="MonoBehaviourActor{TMeta}"/> using <see cref="TransitionalSpriteRenderer"/> to represent appearance of the actor.
    /// </summary>
    public abstract class SpriteActor<TMeta> : MonoBehaviourActor<TMeta>, Blur.IBlurable
        where TMeta : OrthoActorMetadata
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool Visible { get => visible; set => SetVisibility(value); }

        protected virtual LocalizableResourceLoader<Texture2D> AppearanceLoader { get; private set; }
        protected virtual TransitionalRenderer TransitionalRenderer { get; private set; }

        private string appearance;
        private bool visible;
        private Resource<Texture2D> defaultAppearance;

        protected SpriteActor (string id, TMeta metadata)
            : base(id, metadata) { }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            AppearanceLoader = ConstructAppearanceLoader(ActorMetadata);
            TransitionalRenderer = TransitionalRenderer.CreateFor(ActorMetadata, GameObject, false);
            SetVisibility(false);
        }

        public UniTask BlurAsync (float intensity, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            return TransitionalRenderer.BlurAsync(intensity, duration, easingType, asyncToken);
        }

        public override async UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default,
            Transition? transition = default, AsyncToken asyncToken = default)
        {
            var previousAppearance = this.appearance;
            this.appearance = appearance;

            var textureResource = string.IsNullOrWhiteSpace(appearance) 
                ? await LoadDefaultAppearanceAsync(asyncToken) 
                : await LoadAppearanceAsync(appearance, asyncToken);
            AppearanceLoader.Hold(appearance, this);
            await TransitionalRenderer.TransitionToAsync(textureResource, duration, easingType, transition, asyncToken);

            // When using `wait:false` this async method won't be waited, which potentially could lead to a situation, where
            // a consequent same method will re-set the currently disposed resource.
            // Here we check that the disposed (previousAppearance) resource is not actually being used right now, before disposing it.
            if (previousAppearance != this.appearance)
                AppearanceLoader?.Release(previousAppearance, this);
        }

        public override async UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            // When appearance is not set (and default one is not preloaded for some reason, eg when using dynamic parameters) 
            // and revealing the actor -- attempt to load default appearance texture.
            if (!Visible && visible && string.IsNullOrWhiteSpace(Appearance) && (defaultAppearance is null || !defaultAppearance.Valid))
                await ChangeAppearanceAsync(null, 0, asyncToken: asyncToken);

            this.visible = visible;

            await TransitionalRenderer.FadeToAsync(visible ? TintColor.a : 0, duration, easingType, asyncToken);
        }

        public override async UniTask HoldResourcesAsync (string appearance, object holder)
        {
            if (string.IsNullOrEmpty(appearance))
            {
                await LoadDefaultAppearanceAsync(default);
                AppearanceLoader.Hold(defaultAppearance, holder);
                return;
            }

            await AppearanceLoader.LoadAndHoldAsync(appearance, holder);
        }

        public override void ReleaseResources (string appearance, object holder)
        {
            if (string.IsNullOrEmpty(appearance)) return;

            AppearanceLoader?.Release(appearance, holder);
        }

        public override void Dispose ()
        {
            base.Dispose();

            AppearanceLoader?.ReleaseAll(this);
        }

        protected virtual LocalizableResourceLoader<Texture2D> ConstructAppearanceLoader (OrthoActorMetadata metadata)
        {
            var providerManager = Engine.GetService<IResourceProviderManager>();
            var localizationManager = Engine.GetService<ILocalizationManager>();
            var appearanceLoader = new LocalizableResourceLoader<Texture2D>(
                providerManager.GetProviders(metadata.Loader.ProviderTypes), providerManager,
                localizationManager, $"{metadata.Loader.PathPrefix}/{Id}");

            return appearanceLoader;
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).Forget();

        protected virtual void SetVisibility (bool visible) => ChangeVisibilityAsync(visible, 0).Forget();

        protected override Color GetBehaviourTintColor () => TransitionalRenderer.TintColor;

        protected override void SetBehaviourTintColor (Color tintColor)
        {
            if (!Visible) // Handle visibility-controlled alpha of the tint color.
                tintColor.a = TransitionalRenderer.TintColor.a;
            TransitionalRenderer.TintColor = tintColor;
        }

        protected virtual async UniTask<Resource<Texture2D>> LoadAppearanceAsync (string appearance, AsyncToken asyncToken)
        {
            var texture = await AppearanceLoader.LoadAsync(appearance);
            asyncToken.ThrowIfCanceled();

            if (!texture.Valid)
            {
                Debug.LogWarning($"Failed to load '{appearance}' appearance texture for `{Id}` sprite actor: the resource is not found.");
                return null;
            }

            ApplyTextureSettings(texture);
            return texture;
        }

        protected virtual async UniTask<Resource<Texture2D>> LoadDefaultAppearanceAsync (AsyncToken asyncToken)
        {
            if (defaultAppearance != null && defaultAppearance.Valid) return defaultAppearance;

            var defaultTexturePath = await LocateDefaultAppearanceAsync(asyncToken);
            if (!string.IsNullOrEmpty(defaultTexturePath))
            {
                defaultAppearance = await AppearanceLoader.LoadAsync(defaultTexturePath);
                asyncToken.ThrowIfCanceled();
            }
            else defaultAppearance = new Resource<Texture2D>(null, Engine.LoadInternalResource<Texture2D>("Textures/UnknownActor"));

            ApplyTextureSettings(defaultAppearance);

            if (!TransitionalRenderer.MainTexture)
                TransitionalRenderer.MainTexture = defaultAppearance;

            return defaultAppearance;
        }

        protected virtual async UniTask<string> LocateDefaultAppearanceAsync (AsyncToken asyncToken)
        {
            var texturePaths = (await AppearanceLoader.LocateAsync(string.Empty))?.ToList();
            asyncToken.ThrowIfCanceled();
            if (texturePaths != null && texturePaths.Count > 0)
            {
                // First, look for an appearance with a name, equal to actor's ID.
                if (texturePaths.Any(t => t.EqualsFast(Id)))
                    return texturePaths.First(t => t.EqualsFast(Id));

                // Then, try a `Default` appearance.
                if (texturePaths.Any(t => t.EqualsFast("Default")))
                    return texturePaths.First(t => t.EqualsFast("Default"));

                // Finally, fallback to a first defined appearance.
                return texturePaths.FirstOrDefault();
            }

            return null;
        }

        protected virtual void ApplyTextureSettings (Texture2D texture)
        {
            if (texture && texture.wrapMode != TextureWrapMode.Clamp)
                texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
