// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage objects spawned with <see cref="Commands.Spawn"/> commands.
    /// </summary>
    public interface ISpawnManager : IEngineService<SpawnConfiguration>
    {
        /// <summary>
        /// Spawns an object with the provided path.
        /// </summary>
        UniTask<SpawnedObject> SpawnAsync (string path, AsyncToken asyncToken = default);
        /// <summary>
        /// Checks whether an object with the provided path is currently spawned.
        /// </summary>
        bool IsSpawned (string path);
        /// <summary>
        /// Returns a spawned object with the provided path.
        /// </summary>
        SpawnedObject GetSpawned (string path);
        /// <summary>
        /// Returns all the currently spawned objects.
        /// </summary>
        IReadOnlyCollection<SpawnedObject> GetAllSpawned ();
        /// <summary>
        /// Destroys a spawned object with the provided path.
        /// </summary>
        void DestroySpawned (string path);

        /// <summary>
        /// Preloads and holds resources required to spawn an object with the provided path.
        /// </summary>
        UniTask HoldResourcesAsync (string path, object holder);
        /// <summary>
        /// Releases resources required to spawn an object with the provided path.
        /// </summary>
        void ReleaseResources (string path, object holder);
    }
}
