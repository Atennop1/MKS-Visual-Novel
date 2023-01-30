// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class UIConfiguration : Configuration
    {
        [System.Serializable]
        public class FontOption
        {
            [Tooltip("Name (ID) of the font option. Will be displayed in the font settings dropdown list.")]
            public string FontName;
            [Tooltip("Font asset to apply for the uGUI text components when the option is selected. Leave empty if you're not using uGUI text components.")]
            public Font Font;
            [Tooltip("Font asset to apply for the TMPro text components when the option is selected. Leave empty if you're not using TMPro text components.")]
            public TMP_FontAsset TMPFont;
        }

        public const string DefaultPathPrefix = "UI";

        [Tooltip("Configuration of the resource loader used with UI resources.")]
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultPathPrefix };
        [Tooltip("Whether to assign a specific layer to all the UI objects managed by the engine. Required for some of the built-in features, eg `Toggle UI`.")]
        public bool OverrideObjectsLayer = true;
        [Tooltip("When `Override Objects Layer` is enabled, the specified layer will be assigned to all the managed UI objects.")]
        public int ObjectsLayer = 5;
        [Tooltip("Font options, that should be available in the game settings UI (in addition to `Default`) for the player to choose from.")]
        public List<FontOption> FontOptions = default;
        [Tooltip("Name (ID) of a font from `Font Options` to apply by default when the game is first started. When not specified, `Default` font is applied.")]
        public string DefaultFont = default;

        /// <summary>
        /// Returns a font option with the provided name or null, when not found.
        /// </summary>
        public FontOption GetFontOption (string fontName) => FontOptions?.Find(fo => fo.FontName == fontName);
    }
}
