// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="ISpawnManager"/>.
    /// </summary>
    public static class SpawnManagerExtensions
    {
        /// <summary>
        /// Spawns a new object with the provided path or returns one if it's already spawned.
        /// </summary>
        public static async UniTask<SpawnedObject> GetOrSpawnAsync (this ISpawnManager manager, string path,
            AsyncToken asyncToken = default)
        {
            return manager.IsSpawned(path)
                ? manager.GetSpawned(path)
                : await manager.SpawnAsync(path, asyncToken);
        }

        /// <summary>
        /// Spawns an object with the provided path and applies provided spawn parameters.
        /// </summary>
        public static async UniTask<SpawnedObject> SpawnWithParametersAsync (this ISpawnManager manager, string path,
            IReadOnlyList<string> parameters, AsyncToken asyncToken = default)
        {
            var spawnedObject = await manager.SpawnAsync(path, asyncToken);
            spawnedObject.SetSpawnParameters(parameters);
            return spawnedObject;
        }

        /// <summary>
        /// Spawns an object with the provided path, applies provided spawn parameters and waits.
        /// </summary>
        public static async UniTask<SpawnedObject> SpawnWithParametersAndWaitAsync (this ISpawnManager manager, string path,
            IReadOnlyList<string> parameters, AsyncToken asyncToken = default)
        {
            var spawnedObject = await manager.SpawnWithParametersAsync(path, parameters, asyncToken);
            await spawnedObject.AwaitSpawnAsync(asyncToken);
            return spawnedObject;
        }
    }
}
