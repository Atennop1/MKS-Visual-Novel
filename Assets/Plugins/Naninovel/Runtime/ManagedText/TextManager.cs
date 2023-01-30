// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ITextManager"/>
    [InitializeAtRuntime]
    public class TextManager : ITextManager
    {
        public virtual ManagedTextConfiguration Configuration { get; }

        private readonly IResourceProviderManager providersManager;
        private readonly ILocalizationManager localizationManager;
        private readonly HashSet<ManagedTextRecord> records = new HashSet<ManagedTextRecord>();
        private readonly ILookup<string, Type> typesByName = Engine.Types.ToLookup(t => t.FullName);
        private LocalizableResourceLoader<TextAsset> documentLoader;

        public TextManager (ManagedTextConfiguration config, IResourceProviderManager providersManager, ILocalizationManager localizationManager)
        {
            Configuration = config;
            this.providersManager = providersManager;
            this.localizationManager = localizationManager;
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            documentLoader = Configuration.Loader.CreateLocalizableFor<TextAsset>(providersManager, localizationManager);
            localizationManager.AddChangeLocaleTask(ApplyManagedTextAsync);
            return UniTask.CompletedTask;
        }

        public virtual void ResetService () { }

        public virtual void DestroyService ()
        {
            localizationManager?.RemoveChangeLocaleTask(ApplyManagedTextAsync);
            documentLoader?.ReleaseAll(this);
        }

        public virtual string GetRecordValue (string key, string category = ManagedTextRecord.DefaultCategoryName)
        {
            foreach (var record in records)
                if (record.Category.EqualsFast(category) && record.Key.EqualsFast(key))
                    return record.Value;
            return null;
        }

        public virtual IReadOnlyCollection<ManagedTextRecord> GetAllRecords (params string[] categoryFilter)
        {
            if (categoryFilter is null || categoryFilter.Length == 0)
                return records.ToList();

            var result = new List<ManagedTextRecord>();
            foreach (var record in records)
                if (categoryFilter.Contains(record.Category))
                    result.Add(record);
            return result;
        }

        public virtual async UniTask ApplyManagedTextAsync ()
        {
            records.Clear();
            var documentResources = await documentLoader.LoadAndHoldAllAsync(this);
            foreach (var resource in documentResources)
                if (resource.Valid) records.UnionWith(ParseManagedText(resource));
                else Debug.LogWarning($"Failed to load `{resource.Path}` managed text document.");
            foreach (var record in records)
                ApplyRecord(record);
        }

        protected virtual HashSet<ManagedTextRecord> ParseManagedText (Resource<TextAsset> resource)
        {
            var category = documentLoader.GetLocalPath(resource);
            var text = resource.Object.text;
            return ManagedTextUtils.ParseDocument(text, category);
        }

        protected virtual void ApplyRecord (ManagedTextRecord record)
        {
            var typeName = record.Key.GetBeforeLast(".") ?? record.Key;
            var type = typesByName[typeName].FirstOrDefault();
            if (type is null) return;
            var fieldName = record.Key.GetAfter(".") ?? record.Key;
            var fieldInfo = type.GetField(fieldName, ManagedTextUtils.ManagedFieldBindings);
            if (fieldInfo is null) Debug.LogWarning($"Failed to apply managed text record value to '{type.FullName}.{fieldName}' field.");
            else fieldInfo.SetValue(null, record.Value);
        }
    }
}
