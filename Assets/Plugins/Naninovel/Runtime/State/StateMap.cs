// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a map of serializable objects indexed by their assembly-qualified type names.
    /// </summary>
    [Serializable]
    public class StateMap : ISerializationCallbackReceiver
    {
        protected readonly Dictionary<string, object> ObjectMap = new Dictionary<string, object>(StringComparer.Ordinal);

        [SerializeField] private SerializableLiteralStringMap objectJsonMap = new SerializableLiteralStringMap();

        public virtual void OnBeforeSerialize ()
        {
            objectJsonMap.Clear();
            foreach (var kv in ObjectMap)
                objectJsonMap.Add(kv.Key, JsonUtility.ToJson(kv.Value));
        }

        public virtual void OnAfterDeserialize ()
        {
            ObjectMap.Clear();
            foreach (var kv in objectJsonMap)
            {
                var type = Type.GetType(kv.Key);
                if (type is null) continue; // Could (rarely) happen when we change asmdefs for naninovel sources.
                ObjectMap[kv.Key] = JsonUtility.FromJson(kv.Value, type);
            }
        }

        /// <summary>
        /// Stores the provided serializable object in the map using the object's type as the key.
        /// </summary>
        /// <typeparam name="TState">Type of the serialized state.</typeparam>
        /// <param name="state">State object to serialize.</param>
        /// <param name="instanceId">Optional instance ID, use when storing multiple objects of equal type.</param>
        public void SetState<TState> (TState state, string instanceId = default) where TState : class, new()
        {
            var key = typeof(TState).AssemblyQualifiedName;
            if (!string.IsNullOrEmpty(instanceId))
                key += $", InstanceID={instanceId}";
            ObjectMap[key] = state;
        }

        /// <summary>
        /// Attempts to retrieve a serializable object with the provided type from the map. 
        /// Will return null when no objects of the type is contained in the map.
        /// </summary>
        /// <typeparam name="TState">Type of the serialized state.</typeparam>
        /// <param name="instanceId">Optional instance ID, use when storing multiple objects of equal type.</param>
        /// <returns>Deserialized state object or null, when none if found.</returns>
        public TState GetState<TState> (string instanceId = default) where TState : class, new()
        {
            var key = typeof(TState).AssemblyQualifiedName;
            if (!string.IsNullOrEmpty(instanceId))
                key += $", InstanceID={instanceId}";
            return ObjectMap.TryGetValue(key, out var state) ? state as TState : null;
        }
    }
}
