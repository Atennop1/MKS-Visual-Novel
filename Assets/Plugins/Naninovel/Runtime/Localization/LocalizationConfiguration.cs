// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class LocalizationConfiguration : Configuration
    {
        public const string DefaultLocalizationPathPrefix = "Localization";

        [Tooltip("Configuration of the resource loader used with the localization resources.")]
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultLocalizationPathPrefix };
        [Tooltip("Locale of the source project resources (language in which the project assets are being authored).")]
        public string SourceLocale = "en";
        [Tooltip("Locale selected by default when running the game for the first time. Will select `Source Locale` when not specified.")]
        public string DefaultLocale = default;
    }
}
