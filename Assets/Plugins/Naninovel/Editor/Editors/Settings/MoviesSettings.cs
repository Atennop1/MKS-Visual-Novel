// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class MoviesSettings : ResourcefulSettings<MoviesConfiguration>
    {
        protected override string HelpUri => "guide/movies.html";

        protected override Type ResourcesTypeConstraint => typeof(UnityEngine.Video.VideoClip);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override string ResourcesSelectionTooltip => "Use `@movie %name%` in naninovel scripts to play a movie of the selected video clip.";

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(MoviesConfiguration.IntroMovieName)] = p => DrawWhen(Configuration.PlayIntroMovie, p);
            return drawers;
        }

        [MenuItem("Naninovel/Resources/Movies")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
