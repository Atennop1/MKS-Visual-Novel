// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="ILocalizationManager"/>.
    /// </summary>
    public static class LocalizationManagerExtensions
    {
        /// <summary>
        /// Whether <see cref="LocalizationConfiguration.SourceLocale"/> is currently selected.
        /// </summary>
        public static bool IsSourceLocaleSelected (this ILocalizationManager manager) => manager.SelectedLocale == manager.Configuration.SourceLocale;
    }
}
