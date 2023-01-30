// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class UISettings : ResourcefulSettings<UIConfiguration>
    {
        protected override string HelpUri => "guide/user-interface.html#ui-customization";

        protected override Type ResourcesTypeConstraint => typeof(GameObject);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override string ResourcesSelectionTooltip => "Use `@showUI %name%` to show and `@hideUI %name%` to hide the UI.";

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(UIConfiguration.ObjectsLayer)] = p =>
            {
                if (!Configuration.OverrideObjectsLayer) return;
                var label = EditorGUI.BeginProperty(Rect.zero, null, p);
                p.intValue = EditorGUILayout.LayerField(label, p.intValue);
            };
            return drawers;
        }
        
        [MenuItem("Naninovel/Resources/UI")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
