// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using Naninovel.FX;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IActor"/> implementation using <see cref="LayeredActorBehaviour"/> to represent the actor.
    /// </summary>
    public abstract class LayeredActor<TBehaviour, TMeta> : MonoBehaviourActor<TMeta>, Blur.IBlurable
        where TBehaviour : LayeredActorBehaviour
        where TMeta : OrthoActorMetadata
    {
        /// <summary>
        /// Behaviour component of the instantiated layered prefab associated with the actor.
        /// </summary>
        public virtual TBehaviour Behaviour { get; private set; }
        public override string Appearance { get => Behaviour.Composition; set => SetAppearance(value); }
        public override bool Visible { get => visible; set => SetVisibility(value); }

        protected virtual TransitionalRenderer TransitionalRenderer { get; private set; }

        private LocalizableResourceLoader<GameObject> prefabLoader;
        private RenderTexture appearanceTexture;
        private string defaultAppearance;
        private bool visible;

        protected LayeredActor (string id, TMeta metadata)
            : base(id, metadata) { }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            TransitionalRenderer = TransitionalRenderer.CreateFor(ActorMetadata, GameObject, true);

            SetVisibility(false);

            var providerManager = Engine.GetService<IResourceProviderManager>();
            var localizationManager = Engine.GetService<ILocalizationManager>();
            prefabLoader = ActorMetadata.Loader.CreateLocalizableFor<GameObject>(providerManager, localizationManager);

            var prefabResource = await prefabLoader.LoadAndHoldAsync(Id, this);
            // Don't parent prefab to the actor object, as drawer is not handling parent scale.
            var prefabInstance = Engine.Instantiate(prefabResource.Object, $"{Id}LayeredPrefab");
            Behaviour = prefabInstance.GetComponent<TBehaviour>();
            defaultAppearance = Behaviour.Composition;

            // Force render once, otherwise the render texture is initially empty.
            await ChangeAppearanceAsync(defaultAppearance, 0);

            Engine.Behaviour.OnBehaviourUpdate += RenderAppearance;
        }

        public UniTask BlurAsync (float intensity, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            return TransitionalRenderer.BlurAsync(intensity, duration, easingType, asyncToken);
        }

        public override async UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default,
            Transition? transition = default, AsyncToken asyncToken = default)
        {
            if (string.IsNullOrEmpty(appearance))
                appearance = defaultAppearance;

            Behaviour.ApplyComposition(appearance);
            var previousTexture = appearanceTexture;
            appearanceTexture = Behaviour.Render(ActorMetadata.PixelsPerUnit);
            await TransitionalRenderer.TransitionToAsync(appearanceTexture, duration, easingType, transition, asyncToken);

            if (previousTexture)
                RenderTexture.ReleaseTemporary(previousTexture);
        }

        public override async UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            this.visible = visible;

            await TransitionalRenderer.FadeToAsync(visible ? TintColor.a : 0, duration, easingType, asyncToken);
        }

        public override void Dispose ()
        {
            if (Engine.Behaviour != null)
                Engine.Behaviour.OnBehaviourUpdate -= RenderAppearance;

            if (appearanceTexture)
                RenderTexture.ReleaseTemporary(appearanceTexture);

            prefabLoader?.ReleaseAll(this);

            if (Behaviour) ObjectUtils.DestroyOrImmediate(Behaviour.gameObject);

            base.Dispose();
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

        protected virtual void RenderAppearance ()
        {
            if (!Behaviour || !Behaviour.Animated || !appearanceTexture) return;

            var texture = Behaviour.Render(ActorMetadata.PixelsPerUnit, appearanceTexture);
            if (texture != appearanceTexture)
            {
                RenderTexture.ReleaseTemporary(appearanceTexture);
                appearanceTexture = texture;
                TransitionalRenderer.MainTexture = texture;
            }
        }
    }
}
