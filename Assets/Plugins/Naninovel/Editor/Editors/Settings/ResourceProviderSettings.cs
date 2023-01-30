// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class ResourceProviderSettings : ConfigurationSettings<ResourceProviderConfiguration>
    {
        protected override string HelpUri => "guide/resource-providers.html";

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers ()
        {
            var drawers = base.OverrideConfigurationDrawers();
            drawers[nameof(ResourceProviderConfiguration.DynamicPolicySteps)] = p => DrawWhen(Configuration.ResourcePolicy == ResourcePolicy.Dynamic, p);
            drawers[nameof(ResourceProviderConfiguration.OptimizeLoadingPriority)] = p => DrawWhen(Configuration.ResourcePolicy == ResourcePolicy.Dynamic, p);
            drawers[nameof(ResourceProviderConfiguration.UseAddressables)] = p => {
                if (!Configuration.EnableBuildProcessing)
                {
                    EditorGUILayout.HelpBox("While processing is disabled, assets assigned as Naninovel resources may not be available in the build. In case using a custom build handler, consider invoking `BuildProcessor.PreprocessBuild()` and `BuildProcessor.PostprocessBuild()` methods to replicate Naninovel's processing.", MessageType.Warning);
                    return;
                }

                #if ADDRESSABLES_AVAILABLE
                EditorGUILayout.PropertyField(p);
                if (!Configuration.UseAddressables)
                    EditorGUILayout.HelpBox("When `Use Addressables` is disabled, all the assets assigned as Naninovel resources and not stored in `Resources` folders will be copied and re-imported when building the player, which could significantly increase the build time.", MessageType.Warning);
                #else
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(p.displayName, false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("Consider installing the Addressable Asset System (via Unity's package manager). When the system is not available, all the assets assigned as Naninovel resources and not stored in `Resources` folders will be copied and re-imported when building the player, which could significantly increase the build time.", MessageType.Warning);
                #endif
            };
            drawers[nameof(ResourceProviderConfiguration.AutoBuildBundles)] = p => {
                #if ADDRESSABLES_AVAILABLE
                DrawWhen(Configuration.EnableBuildProcessing && Configuration.UseAddressables, p);
                #endif
            };
            return drawers;
        }
    }
}
