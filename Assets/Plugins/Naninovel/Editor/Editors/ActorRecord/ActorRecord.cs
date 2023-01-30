// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Used as an alternative for editing actor metadata in editor menus.
    /// </summary>
    public abstract class ActorRecord : ScriptableObject
    {
        public abstract ActorMetadata GetMetadata ();
    }

    /// <inheritdoc cref="ActorRecord"/>
    public abstract class ActorRecord<TMeta> : ActorRecord
        where TMeta : ActorMetadata
    {
        public TMeta Metadata => metadata;

        public override ActorMetadata GetMetadata () => metadata;

        [SerializeField] private TMeta metadata = default;
    }
}
