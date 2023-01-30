// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ResourceLoader{TResource}"/>, that will attempt to use <see cref="Naninovel.ILocalizationManager"/> to retrieve localized versions 
    /// of the requested resources and (optionally) fallback to the source (original) versions when localized versions are not available.
    /// </summary>
    public class LocalizableResourceLoader<TResource> : ResourceLoader<TResource>
        where TResource : UnityEngine.Object
    {
        /// <summary>
        /// When set, will use the provided locale instead of the <see cref="ILocalizationManager.SelectedLocale"/>.
        /// </summary>
        public string OverrideLocale { get => overrideLocale; set => SetOverrideLocale(value); }

        protected readonly ILocalizationManager LocalizationManager;
        protected readonly List<IResourceProvider> SourceProviders;
        protected readonly string SourcePrefix;
        protected readonly bool FallbackToSource;

        private string overrideLocale;

        /// <param name="providersList">Prioritized list of the source providers.</param>
        /// <param name="localizationManager">Localization manager instance.</param>
        /// <param name="sourcePrefix">Resource path prefix for the source providers.</param>
        /// <param name="fallbackToSource">Whether to fallback to the source versions of the resources when localized versions are not available.</param>
        public LocalizableResourceLoader (List<IResourceProvider> providersList, IHoldersTracker holdersTracker, ILocalizationManager localizationManager,
            string sourcePrefix = null, bool fallbackToSource = true) : base(providersList, holdersTracker, sourcePrefix)
        {
            LocalizationManager = localizationManager;
            SourceProviders = providersList.ToList();
            SourcePrefix = sourcePrefix;
            FallbackToSource = fallbackToSource;

            LocalizationManager.AddChangeLocaleTask(HandleLocaleChangedAsync);
            InitializeProvisionSources();
        }

        ~LocalizableResourceLoader ()
        {
            LocalizationManager?.RemoveChangeLocaleTask(HandleLocaleChangedAsync);
        }

        protected void SetOverrideLocale (string locale)
        {
            if (overrideLocale == locale) return;
            overrideLocale = locale;
            HandleLocaleChangedAsync().Forget();
        }

        protected void InitializeProvisionSources ()
        {
            ProvisionSources.Clear();

            if (!LocalizationManager.IsSourceLocaleSelected() || !string.IsNullOrEmpty(overrideLocale))
            {
                var locale = string.IsNullOrEmpty(overrideLocale) ? LocalizationManager.SelectedLocale : overrideLocale;
                var localePrefix = $"{LocalizationManager.Configuration.Loader.PathPrefix}/{locale}/{SourcePrefix}";
                foreach (var provider in LocalizationManager.ProviderList)
                    ProvisionSources.Add(new ProvisionSource(provider, localePrefix));
            }

            if (FallbackToSource)
                foreach (var provider in SourceProviders)
                    ProvisionSources.Add(new ProvisionSource(provider, SourcePrefix));
        }

        protected async UniTask HandleLocaleChangedAsync ()
        {
            InitializeProvisionSources();

            var tasks = new List<UniTask>();
            foreach (var resource in LoadedResources.ToArray())
                tasks.Add(ReloadIfLocalized(resource));
            await UniTask.WhenAll(tasks);

            async UniTask ReloadIfLocalized (LoadedResource resource)
            {
                if (!resource.Valid || !await IsLocalized(resource)) return;
                LoadedResources.Remove(resource);
                if (HoldersTracker.Release(resource.Object, this) == 0)
                    resource.ProvisionSource.Provider.UnloadResource(resource.FullPath);
                var localizedResource = await LoadAsync(resource.LocalPath);
                HoldersTracker.Hold(localizedResource.Object, this);
                GetLoadedResource(resource.LocalPath).AddHoldersFrom(resource);
            }

            async UniTask<bool> IsLocalized (LoadedResource resource)
            {
                foreach (var source in ProvisionSources)
                {
                    if (source == resource.ProvisionSource) return false;
                    var fullPath = source.BuildFullPath(resource.LocalPath);
                    if (await source.Provider.ResourceExistsAsync<TResource>(fullPath)) return true;
                }
                return false;
            }
        }
    }
}
