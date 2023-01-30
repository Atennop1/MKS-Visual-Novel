// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Instantiates a prefab or a [special effect](/guide/special-effects.md);
    /// when performed over an already spawned object, will update the spawn parameters instead.
    /// </summary>
    /// <remarks>
    /// If prefab has a `MonoBehaviour` component attached the root object, and the component implements
    /// a `IParameterized` interface, will pass the specified `params` values after the spawn;
    /// if the component implements `IAwaitable` interface, command execution will wait for
    /// the async completion task returned by the implementation.
    /// </remarks>
    public class Spawn : Command, Command.IPreloadable
    {
        public interface IParameterized
        {
            void SetSpawnParameters (IReadOnlyList<string> parameters);
        }

        public interface IAwaitable
        {
            UniTask AwaitSpawnAsync (AsyncToken asyncToken = default);
        }

        /// <summary>
        /// Name (path) of the prefab resource to spawn.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ResourceContext(SpawnConfiguration.DefaultPathPrefix)]
        public StringParameter Path;
        /// <summary>
        /// Parameters to set when spawning the prefab.
        /// Requires the prefab to have a `IParameterized` component attached the root object.
        /// </summary>
        public StringListParameter Params;
        /// <summary>
        /// Position (relative to the scene borders, in percents) to set for the spawned object.
        /// Position is described as follows: `0,0` is the bottom left, `50,50` is the center and `100,100` is the top right corner of the scene.
        /// Use Z-component (third member, eg `,,10`) to move (sort) by depth while in ortho mode.
        /// </summary>
        [ParameterAlias("pos")]
        public DecimalListParameter ScenePosition;
        /// <summary>
        /// Position (in world space) to set for the spawned object. 
        /// </summary>
        public DecimalListParameter Position;
        /// <summary>
        /// Rotation to set for the spawned object.
        /// </summary>
        public DecimalListParameter Rotation;
        /// <summary>
        /// Scale to set for the spawned object.
        /// </summary>
        public DecimalListParameter Scale;

        protected virtual ISpawnManager SpawnManager => Engine.GetService<ISpawnManager>();

        public virtual async UniTask PreloadResourcesAsync ()
        {
            if (!Assigned(Path) || Path.DynamicValue || string.IsNullOrWhiteSpace(Path)) return;
            await SpawnManager.HoldResourcesAsync(Path, this);
        }

        public virtual void ReleasePreloadedResources ()
        {
            if (!Assigned(Path) || Path.DynamicValue || string.IsNullOrWhiteSpace(Path)) return;
            SpawnManager.ReleaseResources(Path, this);
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var spawnedObject = await SpawnManager.GetOrSpawnAsync(Path, asyncToken);

            ApplyParameters(spawnedObject);
            ApplyScenePosition(spawnedObject);
            ApplyPosition(spawnedObject);
            ApplyRotation(spawnedObject);
            ApplyScale(spawnedObject);

            await spawnedObject.AwaitSpawnAsync(asyncToken);
        }

        protected virtual void ApplyParameters (SpawnedObject spawnedObject)
        {
            var parameters = Params?.ToReadOnlyList();
            spawnedObject.SetSpawnParameters(parameters);
        }

        protected virtual void ApplyScenePosition (SpawnedObject spawnedObject)
        {
            if (!Assigned(ScenePosition)) return;
            var config = Engine.GetService<ICameraManager>().Configuration;
            spawnedObject.Transform.position = new Vector3(
                ScenePosition.ElementAtOrDefault(0) != null
                    ? config.SceneToWorldSpace(new Vector2(ScenePosition[0] / 100f, 0)).x
                    : spawnedObject.Transform.position.x,
                ScenePosition.ElementAtOrDefault(1) != null
                    ? config.SceneToWorldSpace(new Vector2(0, ScenePosition[1] / 100f)).y
                    : spawnedObject.Transform.position.y,
                ScenePosition.ElementAtOrDefault(2) ?? spawnedObject.Transform.position.z);
        }

        protected virtual void ApplyPosition (SpawnedObject spawnedObject)
        {
            if (!Assigned(Position)) return;
            spawnedObject.Transform.position = new Vector3(
                Position.ElementAtOrDefault(0) ?? spawnedObject.Transform.position.x,
                Position.ElementAtOrDefault(1) ?? spawnedObject.Transform.position.y,
                Position.ElementAtOrDefault(2) ?? spawnedObject.Transform.position.z);
        }

        protected virtual void ApplyRotation (SpawnedObject spawnedObject)
        {
            if (!Assigned(Rotation)) return;
            spawnedObject.Transform.rotation = Quaternion.Euler(
                Rotation.ElementAtOrDefault(0) ?? spawnedObject.Transform.eulerAngles.x,
                Rotation.ElementAtOrDefault(1) ?? spawnedObject.Transform.eulerAngles.y,
                Rotation.ElementAtOrDefault(2) ?? spawnedObject.Transform.eulerAngles.z);
        }

        protected virtual void ApplyScale (SpawnedObject spawnedObject)
        {
            if (!Assigned(Scale)) return;

            if (Scale.Length == 1 && Scale[0].HasValue)
                spawnedObject.Transform.localScale = new Vector3(Scale[0], Scale[0], Scale[0]);
            else
                spawnedObject.Transform.localScale = new Vector3(
                    Scale.ElementAtOrDefault(0) ?? spawnedObject.Transform.localScale.x,
                    Scale.ElementAtOrDefault(1) ?? spawnedObject.Transform.localScale.y,
                    Scale.ElementAtOrDefault(2) ?? spawnedObject.Transform.localScale.z);
        }
    }
}
