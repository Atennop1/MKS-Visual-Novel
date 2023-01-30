// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="IChoiceHandlerManager"/>
    [InitializeAtRuntime]
    public class ChoiceHandlerManager : ActorManager<IChoiceHandlerActor, ChoiceHandlerState, ChoiceHandlerMetadata, ChoiceHandlersConfiguration>, IChoiceHandlerManager
    {
        public virtual IResourceLoader<GameObject> ChoiceButtonLoader => buttonLoader;

        private readonly IResourceProviderManager providerManager;
        private readonly ILocalizationManager localizationManager;

        private LocalizableResourceLoader<GameObject> buttonLoader;

        public ChoiceHandlerManager (ChoiceHandlersConfiguration config, IResourceProviderManager providerManager, ILocalizationManager localizationManager)
            : base(config)
        {
            this.providerManager = providerManager;
            this.localizationManager = localizationManager;
        }

        public override async UniTask InitializeServiceAsync ()
        {
            await base.InitializeServiceAsync();
            buttonLoader = Configuration.ChoiceButtonLoader.CreateLocalizableFor<GameObject>(providerManager, localizationManager);
        }

        public override void DestroyService ()
        {
            base.DestroyService();
            ChoiceButtonLoader?.ReleaseAll(this);
        }
    }
}
