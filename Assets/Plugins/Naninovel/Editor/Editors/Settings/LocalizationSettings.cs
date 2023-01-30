// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class LocalizationSettings : ConfigurationSettings<LocalizationConfiguration>
    {
        protected override string HelpUri => "guide/localization.html";

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(LocalizationConfiguration.SourceLocale)] = p => LocalesPopupDrawer.Draw(p);
            drawers[nameof(LocalizationConfiguration.DefaultLocale)] = p => LocalesPopupDrawer.Draw(p, true);
            return drawers;
        }

        protected override void DrawConfigurationEditor ()
        {
            DrawDefaultEditor();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Localization Utility", GUIStyles.NavigationButton))
                LocalizationWindow.OpenWindow();
        }
    }
}
