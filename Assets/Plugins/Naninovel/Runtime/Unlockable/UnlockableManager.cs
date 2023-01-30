// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <inheritdoc cref="IUnlockableManager"/>
    [InitializeAtRuntime]
    public class UnlockableManager : IUnlockableManager, IStatefulService<GlobalStateMap>
    {
        /// <summary>
        /// Serializable dictionary, representing unlockable item ID to its unlocked state map.
        /// </summary>
        [Serializable]
        public class UnlockablesMap : SerializableMap<string, bool>
        {
            public UnlockablesMap () : base(StringComparer.OrdinalIgnoreCase) { }
            public UnlockablesMap (UnlockablesMap map) : base(map, StringComparer.OrdinalIgnoreCase) { }
        }

        [Serializable]
        public class GlobalState
        {
            public UnlockablesMap UnlockablesMap = new UnlockablesMap();
        }

        public event Action<UnlockableItemUpdatedArgs> OnItemUpdated;

        public virtual UnlockablesConfiguration Configuration { get; }

        private UnlockablesMap unlockablesMap;

        public UnlockableManager (UnlockablesConfiguration config)
        {
            Configuration = config;
            unlockablesMap = new UnlockablesMap();
        }

        public virtual UniTask InitializeServiceAsync () => UniTask.CompletedTask;

        public virtual void ResetService () { }

        public virtual void DestroyService () { }

        public virtual void SaveServiceState (GlobalStateMap stateMap)
        {
            var globalState = new GlobalState {
                UnlockablesMap = new UnlockablesMap(unlockablesMap)
            };
            stateMap.SetState(globalState);
        }

        public virtual UniTask LoadServiceStateAsync (GlobalStateMap stateMap)
        {
            var state = stateMap.GetState<GlobalState>();
            if (state is null) return UniTask.CompletedTask;

            unlockablesMap = new UnlockablesMap(state.UnlockablesMap);
            return UniTask.CompletedTask;
        }

        public virtual bool ItemUnlocked (string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentNullException(nameof(itemId), "Can't get unlock status of item with empty ID.");
            return unlockablesMap.TryGetValue(itemId, out var item) && item;
        }

        public virtual void SetItemUnlocked (string itemId, bool unlocked)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentNullException(nameof(itemId), "Can't set unlock status of item with empty ID.");
            
            if (unlocked && ItemUnlocked(itemId)) return;
            if (!unlocked && unlockablesMap.ContainsKey(itemId) && !ItemUnlocked(itemId)) return;

            var added = unlockablesMap.ContainsKey(itemId);
            unlockablesMap[itemId] = unlocked;
            OnItemUpdated?.Invoke(new UnlockableItemUpdatedArgs(itemId, unlocked, added));
        }

        public virtual void UnlockItem (string itemId) => SetItemUnlocked(itemId, true);

        public virtual void LockItem (string itemId) => SetItemUnlocked(itemId, false);

        public virtual Dictionary<string, bool> GetAllItems () => unlockablesMap.ToDictionary(kv => kv.Key, kv => kv.Value);

        public virtual void UnlockAllItems ()
        {
            foreach (var itemId in unlockablesMap.Keys.ToArray())
                UnlockItem(itemId);
        }

        public virtual void LockAllItems ()
        {
            foreach (var itemId in unlockablesMap.Keys.ToArray())
                LockItem(itemId);
        }
    }
}
