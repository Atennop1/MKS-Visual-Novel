// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage the localization activities.
    /// </summary>
    public interface ILocalizationManager : IEngineService<LocalizationConfiguration>
    {
        /// <summary>
        /// Event invoked when the locale is changed.
        /// </summary>
        event Action<string> OnLocaleChanged;

        /// <summary>
        /// Language tag of the currently selected localization.
        /// </summary>
        string SelectedLocale { get; }
        /// <summary>
        /// Resource provider list used to access the localization resources.
        /// </summary>
        List<IResourceProvider> ProviderList { get; }

        /// <summary>
        /// Returns language tags of the available localizations.
        /// </summary>
        IReadOnlyCollection<string> GetAvailableLocales ();
        /// <summary>
        /// Whether localization with the provided language tag is available.
        /// </summary>
        bool LocaleAvailable (string locale);
        /// <summary>
        /// Selects (switches to) localization with the provided language tag.
        /// </summary>
        UniTask SelectLocaleAsync (string locale);
        /// <summary>
        /// Adds an async delegate to invoke after changing a locale.
        /// </summary>
        void AddChangeLocaleTask (Func<UniTask> taskFunc);
        /// <summary>
        /// Removes an async delegate to invoke after changing a locale.
        /// </summary>
        void RemoveChangeLocaleTask (Func<UniTask> taskFunc);
    }
}
