// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ILocalizationManager"/>
    [InitializeAtRuntime]
    public class LocalizationManager : IStatefulService<SettingsStateMap>, ILocalizationManager
    {
        [Serializable]
        public class Settings
        {
            public string SelectedLocale;
        }

        public event Action<string> OnLocaleChanged;

        public List<IResourceProvider> ProviderList { get; private set; }
        public virtual LocalizationConfiguration Configuration { get; }
        public virtual string SelectedLocale { get; private set; }

        protected virtual List<string> AvailableLocales { get; } = new List<string>();

        private readonly IResourceProviderManager providersManager;
        private readonly HashSet<Func<UniTask>> changeLocaleTasks = new HashSet<Func<UniTask>>();

        public LocalizationManager (LocalizationConfiguration config, IResourceProviderManager providersManager)
        {
            Configuration = config;
            this.providersManager = providersManager;
        }

        public virtual async UniTask InitializeServiceAsync ()
        {
            ProviderList = providersManager.GetProviders(Configuration.Loader.ProviderTypes);
            await RetrieveAvailableLocalesAsync();
        }

        public virtual void ResetService () { }

        public virtual void DestroyService () { }

        public virtual void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                SelectedLocale = SelectedLocale
            };
            stateMap.SetState(settings);
        }

        public virtual async UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var defaultLocale = string.IsNullOrEmpty(Configuration.DefaultLocale) ? Configuration.SourceLocale : Configuration.DefaultLocale;
            var settings = stateMap.GetState<Settings>() ?? new Settings { SelectedLocale = defaultLocale };
            await SelectLocaleAsync(settings.SelectedLocale ?? defaultLocale);
        }

        public virtual IReadOnlyCollection<string> GetAvailableLocales () => AvailableLocales.ToArray();

        public virtual bool LocaleAvailable (string locale) => AvailableLocales.Contains(locale);

        public virtual async UniTask SelectLocaleAsync (string locale)
        {
            if (!LocaleAvailable(locale))
            {
                Debug.LogWarning($"Failed to select locale: Locale `{locale}` is not available.");
                return;
            }

            if (locale == SelectedLocale) return;

            SelectedLocale = locale;

            foreach (var task in changeLocaleTasks)
                await task();

            OnLocaleChanged?.Invoke(SelectedLocale);
        }

        public virtual void AddChangeLocaleTask (Func<UniTask> taskFunc) => changeLocaleTasks.Add(taskFunc);

        public virtual void RemoveChangeLocaleTask (Func<UniTask> taskFunc) => changeLocaleTasks.Remove(taskFunc);

        /// <summary>
        /// Retrieves available localizations by locating folders inside the localization resources root.
        /// Folder names should correspond to the <see cref="LanguageTags"/> tag entries (RFC5646).
        /// </summary>
        private async UniTask RetrieveAvailableLocalesAsync ()
        {
            var resources = await ProviderList.LocateFoldersAsync(Configuration.Loader.PathPrefix);
            AvailableLocales.Clear();
            AvailableLocales.AddRange(resources.Select(r => r.Name).Where(LanguageTags.ContainsTag));
            AvailableLocales.Add(Configuration.SourceLocale);
        }
    }
}
