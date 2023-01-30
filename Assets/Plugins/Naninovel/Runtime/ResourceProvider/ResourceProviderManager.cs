// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="IResourceProviderManager"/>
    [InitializeAtRuntime]
    public class ResourceProviderManager : IResourceProviderManager
    {
        public event Action<string> OnProviderMessage;

        public virtual ResourceProviderConfiguration Configuration { get; }

        private readonly Dictionary<string, IResourceProvider> providersMap = new Dictionary<string, IResourceProvider>();
        private readonly Dictionary<UnityEngine.Object, HashSet<object>> holdersMap = new Dictionary<UnityEngine.Object, HashSet<object>>();

        public ResourceProviderManager (ResourceProviderConfiguration config)
        {
            Configuration = config;

            if (config.ResourcePolicy == ResourcePolicy.Dynamic && config.OptimizeLoadingPriority)
                Application.backgroundLoadingPriority = ThreadPriority.Low;
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            if (Configuration.MasterProvider != null)
                Configuration.MasterProvider.OnMessage += message => HandleProviderMessage(Configuration.MasterProvider, message);

            Application.lowMemory += HandleLowMemoryAsync;
            return UniTask.CompletedTask;
        }

        public virtual void ResetService () { }

        public virtual void DestroyService ()
        {
            Application.lowMemory -= HandleLowMemoryAsync;
            foreach (var provider in providersMap.Values)
                provider?.UnloadResources();
            Configuration.MasterProvider?.UnloadResources();
        }

        public virtual bool IsProviderInitialized (string providerType) => providersMap.ContainsKey(providerType);

        public virtual IResourceProvider GetProvider (string providerType)
        {
            if (!providersMap.ContainsKey(providerType))
                providersMap[providerType] = InitializeProvider(providerType);
            return providersMap[providerType];
        }

        public virtual List<IResourceProvider> GetProviders (List<string> providerTypes)
        {
            var result = new List<IResourceProvider>();

            // Include editor provider if assigned.
            if (Configuration.MasterProvider != null)
                result.Add(Configuration.MasterProvider);

            // Include requested providers in order.
            foreach (var providerType in providerTypes.Distinct())
            {
                var provider = GetProvider(providerType);
                if (provider != null) result.Add(provider);
            }

            return result;
        }

        public virtual int Hold (UnityEngine.Object obj, object holder)
        {
            var holders = GetHolders(obj);
            holders.Add(holder);
            return holders.Count;
        }

        public virtual int Release (UnityEngine.Object obj, object holder)
        {
            var holders = GetHolders(obj);
            holders.Remove(holder);
            return holders.Count;
        }

        public virtual int CountHolders (UnityEngine.Object obj)
        {
            return GetHolders(obj).Count;
        }

        protected virtual IResourceProvider InitializeProjectProvider ()
        {
            var projectProvider = new ProjectResourceProvider(Configuration.ProjectRootPath);
            return projectProvider;
        }

        protected virtual IResourceProvider InitializeGoogleDriveProvider ()
        {
            #if UNITY_GOOGLE_DRIVE_AVAILABLE
            var gDriveProvider = new GoogleDriveResourceProvider(Configuration.GoogleDriveRootPath, Configuration.GoogleDriveCachingPolicy, Configuration.GoogleDriveRequestLimit);
            gDriveProvider.AddConverter(new JpgOrPngToTextureConverter());
            gDriveProvider.AddConverter(new GDocToScriptAssetConverter());
            gDriveProvider.AddConverter(new Mp3ToAudioClipConverter());
            return gDriveProvider;
            #else
            return null;
            #endif
        }

        protected virtual IResourceProvider InitializeLocalProvider ()
        {
            var localProvider = new LocalResourceProvider(Configuration.LocalRootPath);
            localProvider.AddConverter(new JpgOrPngToTextureConverter());
            localProvider.AddConverter(new NaniToScriptAssetConverter());
            localProvider.AddConverter(new WavToAudioClipConverter());
            localProvider.AddConverter(new Mp3ToAudioClipConverter());
            return localProvider;
        }

        protected virtual IResourceProvider InitializeAddressableProvider ()
        {
            #if ADDRESSABLES_AVAILABLE
            if (Application.isEditor && !Configuration.AllowAddressableInEditor) return null; // Otherwise could be issues with addressables added on previous build, but renamed after.
            var extraLabels = Configuration.ExtraLabels != null && Configuration.ExtraLabels.Length > 0 ? Configuration.ExtraLabels : null;
            return new AddressableResourceProvider(ResourceProviderConfiguration.AddressableId, extraLabels);
            #else
            return null;
            #endif
        }

        protected virtual IResourceProvider InitializeProvider (string providerType)
        {
            IResourceProvider provider;

            switch (providerType)
            {
                case ResourceProviderConfiguration.ProjectTypeName:
                    provider = InitializeProjectProvider();
                    break;
                case ResourceProviderConfiguration.AddressableTypeName:
                    provider = InitializeAddressableProvider();
                    break;
                case ResourceProviderConfiguration.LocalTypeName:
                    provider = InitializeLocalProvider();
                    break;
                case ResourceProviderConfiguration.GoogleDriveTypeName:
                    provider = InitializeGoogleDriveProvider();
                    break;
                default:
                    var customType = Type.GetType(providerType);
                    if (customType is null) throw new Exception($"Failed to initialize '{providerType}' resource provider. Make sure provider types are set correctly in `Loader` properties of the Naninovel configuration menus.");
                    provider = (IResourceProvider)Activator.CreateInstance(customType);
                    if (provider is null) throw new Exception($"Failed to initialize '{providerType}' custom resource provider. Make sure the implementation has a parameterless constructor.");
                    return provider;
            }

            if (provider != null)
                provider.OnMessage += message => HandleProviderMessage(provider, message);

            return provider;
        }

        private void HandleProviderMessage (IResourceProvider provider, string message)
        {
            OnProviderMessage?.Invoke($"[{provider.GetType().Name}] {message}");
        }

        private async void HandleLowMemoryAsync ()
        {
            Debug.LogWarning("Forcing resource unloading due to out of memory.");
            await Resources.UnloadUnusedAssets();
        }

        private HashSet<object> GetHolders (UnityEngine.Object obj)
        {
            if (holdersMap.TryGetValue(obj, out var holders)) return holders;
            holders = new HashSet<object>();
            holdersMap[obj] = holders;
            return holders;
        }
    }
}
