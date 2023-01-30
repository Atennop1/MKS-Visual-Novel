// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#if ADDRESSABLES_AVAILABLE

using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Naninovel
{
    public class AddressableBuilder : IAddressableBuilder
    {
        private readonly ResourceProviderConfiguration config;
        private readonly AddressableAssetSettings settings;

        public AddressableBuilder (ResourceProviderConfiguration config)
        {
            this.config = config;
            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        }

        public void RemoveEntries ()
        {
            foreach (var group in settings.groups)
                if (group && group.Name.StartsWithFast(ResourceProviderConfiguration.AddressableId))
                    foreach (var entry in group.entries.ToArray())
                        group.RemoveAssetEntry(entry);
        }

        public bool TryAddEntry (string assetGuid, string resourcePath)
        {
            if (IsAlreadyAdded(assetGuid, resourcePath)) return false;
            CreateOrUpdateEntry(assetGuid, resourcePath);
            return true;
        }

        public void BuildContent ()
        {
            AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
            AddressableAssetSettings.BuildPlayerContent();
        }

        private bool IsAlreadyAdded (string assetGuid, string resourcePath)
        {
            var entry = settings.FindAssetEntry(assetGuid);
            if (entry is null) return false;
            var address = PathToAddress(resourcePath);
            if (entry.address == address) return false;
            Debug.Log($"Asset assigned as a `{resourcePath}` Naninovel resource is already registered in the Addressable Asset System as `{entry.address}`. It will be copied to prevent conflicts.");
            return true;
        }

        private void CreateOrUpdateEntry (string assetGuid, string resourcePath)
        {
            var address = PathToAddress(resourcePath);
            var groupName = config.GroupByCategory ? PathToGroup(resourcePath) : ResourceProviderConfiguration.AddressableId;
            var group = FindOrCreateGroup(groupName);
            var entry = settings.CreateOrMoveEntry(assetGuid, group);
            entry.SetAddress(address);
            entry.SetLabel(ResourceProviderConfiguration.AddressableId, true, true);
            EditorUtility.SetDirty(settings);
        }

        private AddressableAssetGroup FindOrCreateGroup (string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group != null) return group;
            return settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
        }

        private static string PathToAddress (string path)
        {
            return PathUtils.Combine(ResourceProviderConfiguration.AddressableId, path);
        }

        private static string AddressToPath (string address)
        {
            return address.GetAfterFirst($"{ResourceProviderConfiguration.AddressableId}/");
        }

        private static string PathToGroup (string path)
        {
            var postfix = path.Contains("/") ? path.GetBefore("/") : path;
            return $"{ResourceProviderConfiguration.AddressableId}-{postfix}";
        }
    }
}

#endif
