// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    /// <remarks>
    /// On build pre-process: 
    ///   - When addressable provider is used: assign an addressable address and label to the assets referenced in <see cref="EditorResources"/>;
    ///   - Otherwise: copy the <see cref="EditorResources"/> assets to a temp `Resources` folder (except the assets already stored in `Resources` folders).
    /// On build post-process or build fail: 
    ///   - restore any affected assets and delete the created temporary `Resources` folder.
    /// </remarks>
    public static class BuildProcessor
    {
        /// <summary>
        /// Whether the build processor is currently running.
        /// </summary>
        public static bool Building { get; private set; }

        public static void PreprocessBuild (BuildPlayerOptions options)
        {
            var config = ProjectConfigurationProvider.LoadOrDefault<ResourceProviderConfiguration>();
            var addressableBuilder = CreateAddressableBuilder(config);
            var builder = new ResourcesBuilder(config, addressableBuilder);
            builder.Build(options);
        }

        public static void PostprocessBuild () => ResourcesBuilder.Cleanup();

        [InitializeOnLoadMethod]
        private static void Initialize ()
        {
            var config = ProjectConfigurationProvider.LoadOrDefault<ResourceProviderConfiguration>();
            if (config.EnableBuildProcessing)
                BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayer);
        }

        private static void BuildPlayer (BuildPlayerOptions options)
        {
            try
            {
                Building = true;
                PreprocessBuild(options);
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            }
            finally
            {
                PostprocessBuild();
                Building = false;
            }
        }

        private static IAddressableBuilder CreateAddressableBuilder (ResourceProviderConfiguration config)
        {
            #if ADDRESSABLES_AVAILABLE
            if (config.UseAddressables) return new AddressableBuilder(config);
            #endif
            Debug.Log("Consider installing Addressable Asset System and enabling `Use Addressables` in Naninovel's `Resource Provider` configuration menu. When the system is not available, all the assets assigned as Naninovel resources and not stored in `Resources` folders will be copied and re-imported when building the player, which could significantly increase build time. Check https://naninovel.com/guide/resource-providers.html#addressable for more info.");
            return new MockAddressableBuilder();
        }
    }
}
