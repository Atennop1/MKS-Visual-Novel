// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class StateSettings : ConfigurationSettings<StateConfiguration>
    {
        private static readonly string[] gameImplementations, gameLabels;
        private static readonly string[] globalImplementations, globalLabels;
        private static readonly string[] settingsImplementations, settingsLabels;

        static StateSettings ()
        {
            InitializeImplementationOptions<ISaveSlotManager<GameStateMap>>(ref gameImplementations, ref gameLabels);
            InitializeImplementationOptions<ISaveSlotManager<GlobalStateMap>>(ref globalImplementations, ref globalLabels);
            InitializeImplementationOptions<ISaveSlotManager<SettingsStateMap>>(ref settingsImplementations, ref settingsLabels);
        }

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(StateConfiguration.StateRollbackSteps)] = p => DrawWhen(Configuration.EnableStateRollback, p);
            drawers[nameof(StateConfiguration.SavedRollbackSteps)] = p => DrawWhen(Configuration.EnableStateRollback, p);
            drawers[nameof(StateConfiguration.GameStateHandler)] = p => {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Serialization Handlers", EditorStyles.boldLabel);
                DrawImplementations(gameImplementations, gameLabels, p);
            };
            drawers[nameof(StateConfiguration.GlobalStateHandler)] = p => DrawImplementations(globalImplementations, globalLabels, p);
            drawers[nameof(StateConfiguration.SettingsStateHandler)] = p => DrawImplementations(settingsImplementations, settingsLabels, p);
            return drawers;
        }
    }
}
