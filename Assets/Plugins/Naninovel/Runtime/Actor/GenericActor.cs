// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IActor"/> implementation using <typeparamref name="TBehaviour"/> to represent the actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have a <typeparamref name="TBehaviour"/> component attached to the root object.
    /// Appearance and other property changes changes are routed to the events of the <typeparamref name="TBehaviour"/> component.
    /// </remarks>
    public abstract class GenericActor<TBehaviour, TMeta> : MonoBehaviourActor<TMeta>
        where TBehaviour : GenericActorBehaviour
        where TMeta : ActorMetadata
    {
        /// <summary>
        /// Behaviour component of the instantiated generic prefab associated with the actor.
        /// </summary>
        public virtual TBehaviour Behaviour { get; private set; }
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool Visible { get => visible; set => SetVisibility(value); }

        private LocalizableResourceLoader<GameObject> prefabLoader;
        private string appearance;
        private bool visible;
        private Color tintColor = Color.white;

        protected GenericActor (string id, TMeta metadata)
            : base(id, metadata) { }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            var providerManager = Engine.GetService<IResourceProviderManager>();
            var localizationManager = Engine.GetService<ILocalizationManager>();
            prefabLoader = ActorMetadata.Loader.CreateLocalizableFor<GameObject>(providerManager, localizationManager);
            var prefabResource = await prefabLoader.LoadAndHoldAsync(Id, this);
            if (!prefabResource.Valid) 
                throw new Exception($"Failed to load `{Id}` generic actor prefab. Make sure a valid prefab is assigned in the resources editor menu.");

            Behaviour = Engine.Instantiate(prefabResource.Object).GetComponent<TBehaviour>();
            Behaviour.transform.SetParent(Transform);

            SetVisibility(false);
        }

        public override UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default,
            Transition? transition = default, AsyncToken asyncToken = default)
        {
            SetAppearance(appearance);
            return UniTask.CompletedTask;
        }

        public override UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            SetVisibility(visible);
            return UniTask.CompletedTask;
        }

        protected virtual void SetAppearance (string appearance)
        {
            this.appearance = appearance;

            if (string.IsNullOrEmpty(appearance))
                return;

            Behaviour.InvokeAppearanceChangedEvent(appearance);
        }

        protected virtual void SetVisibility (bool visible)
        {
            this.visible = visible;

            Behaviour.InvokeVisibilityChangedEvent(visible);
        }

        protected override Color GetBehaviourTintColor () => tintColor;

        protected override void SetBehaviourTintColor (Color tintColor)
        {
            this.tintColor = tintColor;

            Behaviour.InvokeTintColorChangedEvent(tintColor);
        }

        public override void Dispose ()
        {
            prefabLoader?.ReleaseAll(this);
            
            base.Dispose();
        }
    }
}
