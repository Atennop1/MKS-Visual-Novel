// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using Naninovel.Commands;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents an object spawned by <see cref="ISpawnManager"/>.
    /// </summary>
    public class SpawnedObject
    {
        /// <summary>
        /// Path (ID) of the spawned object.
        /// </summary>
        public string Path { get; }
        /// <summary>
        /// Spawned game object.
        /// </summary>
        public GameObject GameObject { get; }
        /// <summary>
        /// Transform of the spawned object.
        /// </summary>
        public Transform Transform => GameObject.transform;
        /// <summary>
        /// Current spawn parameter values of the object.
        /// </summary>
        public IReadOnlyList<string> Parameters => parameters;

        private readonly List<string> parameters = new List<string>();
        private readonly Spawn.IParameterized spawnParameterized;
        private readonly Spawn.IAwaitable spawnAwaitable;
        private readonly DestroySpawned.IParameterized destroyParameterized;
        private readonly DestroySpawned.IAwaitable destroyAwaitable;

        public SpawnedObject (string path, GameObject gameObject)
        {
            Path = path;
            GameObject = gameObject;
            gameObject.TryGetComponent<Spawn.IParameterized>(out spawnParameterized);
            gameObject.TryGetComponent<Spawn.IAwaitable>(out spawnAwaitable);
            gameObject.TryGetComponent<DestroySpawned.IParameterized>(out destroyParameterized);
            gameObject.TryGetComponent<DestroySpawned.IAwaitable>(out destroyAwaitable);
        }

        public void SetSpawnParameters (IReadOnlyList<string> value)
        {
            parameters.Clear();
            if (value?.Count > 0) parameters.AddRange(value);
            spawnParameterized?.SetSpawnParameters(value);
        }

        public async UniTask AwaitSpawnAsync (AsyncToken asyncToken = default)
        {
            if (spawnAwaitable != null)
                await spawnAwaitable.AwaitSpawnAsync(asyncToken);
        }

        public void SetDestroyParameters (IReadOnlyList<string> value)
        {
            destroyParameterized?.SetDestroyParameters(value);
        }

        public async UniTask AwaitDestroyAsync (AsyncToken asyncToken = default)
        {
            if (destroyAwaitable != null)
                await destroyAwaitable.AwaitDestroyAsync(asyncToken);
        }
    }
}
