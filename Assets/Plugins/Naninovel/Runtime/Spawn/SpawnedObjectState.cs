// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public struct SpawnedObjectState : IEquatable<SpawnedObjectState>
    {
        public string Path => path;
        public IReadOnlyList<string> Parameters => parameters?.Select(s => s?.Value).ToArray();
        public Vector3 Position => position;
        public Quaternion Rotation => rotation;
        public Vector3 Scale => scale;

        [SerializeField] private string path;
        [SerializeField] private NullableString[] parameters;
        [SerializeField] private Vector3 position;
        [SerializeField] private Quaternion rotation;
        [SerializeField] private Vector3 scale;

        public SpawnedObjectState (SpawnedObject spawnedObject)
        {
            path = spawnedObject.Path;
            parameters = spawnedObject.Parameters?.Select(s => (NullableString)s).ToArray();
            position = spawnedObject.GameObject.transform.position;
            rotation = spawnedObject.GameObject.transform.rotation;
            scale = spawnedObject.GameObject.transform.localScale;
        }

        public void ApplyTo (SpawnedObject spawnedObject)
        {
            if (!spawnedObject.Path.EqualsFast(Path))
                throw new Exception($"Failed to apply `{Path}` spawned object state to `{spawnedObject.Path}`: paths are different.");
            spawnedObject.SetSpawnParameters(Parameters);
            spawnedObject.GameObject.transform.position = Position;
            spawnedObject.GameObject.transform.rotation = Rotation;
            spawnedObject.GameObject.transform.localScale = Scale;
        }

        public bool Equals (SpawnedObjectState other) => path == other.path;
        public override bool Equals (object obj) => obj is SpawnedObjectState other && Equals(other);
        public override int GetHashCode () => Path != null ? Path.GetHashCode() : 0;
        public static bool operator == (SpawnedObjectState left, SpawnedObjectState right) => left.Equals(right);
        public static bool operator != (SpawnedObjectState left, SpawnedObjectState right) => !left.Equals(right);
    }
}
