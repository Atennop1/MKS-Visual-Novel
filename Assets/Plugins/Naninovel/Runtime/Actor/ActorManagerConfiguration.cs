// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Asset used to configure <see cref="IActorManager"/> services.
    /// </summary>
    public abstract class ActorManagerConfiguration : Configuration
    {
        [Tooltip("Default duration (in seconds) for all the actor modifications (changing appearance, position, tint, etc).")]
        public float DefaultDuration = .35f;
        [Tooltip("Easing function to use by default for all the actor modification animations (changing appearance, position, tint, etc).")]
        public EasingType DefaultEasing = EasingType.Linear;
        [Tooltip("Whether to automatically reveal (show) an actor when executing modification commands.")]
        public bool AutoShowOnModify = true;

        public abstract ActorMetadataMap MetadataMap { get; }
        
        /// <summary>
        /// Attempts to retrieve metadata of an actor with the provided ID;
        /// when not found, will return a default metadata.
        /// </summary>
        public ActorMetadata GetMetadataOrDefault (string actorId) => GetMetadataOrDefaultNonGeneric(actorId);

        protected abstract ActorMetadata GetMetadataOrDefaultNonGeneric (string actorId);
    }

    /// <summary>
    /// Asset used to configure <see cref="IActorManager"/> services.
    /// </summary>
    /// <typeparam name="TMeta">Type of actor metadata configured service operates with.</typeparam>
    public abstract class ActorManagerConfiguration<TMeta> : ActorManagerConfiguration
        where TMeta : ActorMetadata
    {
        public abstract TMeta DefaultActorMetadata { get; }
        public abstract ActorMetadataMap<TMeta> ActorMetadataMap { get; }
        public override ActorMetadataMap MetadataMap => ActorMetadataMap;

        /// <inheritdoc cref="ActorManagerConfiguration.GetMetadataOrDefault"/>
        public new TMeta GetMetadataOrDefault (string actorId)
        {
            return ActorMetadataMap.ContainsId(actorId) ? ActorMetadataMap[actorId] : DefaultActorMetadata;
        }

        protected override ActorMetadata GetMetadataOrDefaultNonGeneric (string actorId) => GetMetadataOrDefault(actorId);
    }
}
