// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class EngineSettings : ConfigurationSettings<EngineConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(EngineConfiguration.CustomInitializationUI)] = p => DrawWhen(Configuration.ShowInitializationUI, p);
            drawers[nameof(EngineConfiguration.ObjectsLayer)] = property => {
                if (!Configuration.OverrideObjectsLayer) return;
                var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                property.intValue = EditorGUILayout.LayerField(label, property.intValue);
            };
            drawers[nameof(EngineConfiguration.EnableBridging)] = p => OnChanged(BridgingService.RestartServer, p);
            drawers[nameof(EngineConfiguration.ServerPort)] = p => DrawWhen(Configuration.EnableBridging, p);
            drawers[nameof(EngineConfiguration.AutoGenerateMetadata)] = p => DrawWhen(Configuration.EnableBridging, p);
            drawers[nameof(EngineConfiguration.ToggleConsoleKey)] = p => DrawWhen(Configuration.EnableDevelopmentConsole, p);
            return drawers;
        }
    }
}
