// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="GenericBackgroundBehaviour"/> to represent the actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have a <see cref="GenericBackgroundBehaviour"/> component attached to the root object.
    /// Appearance and other property changes changes are routed to the events of the <see cref="GenericBackgroundBehaviour"/> component.
    /// </remarks>
    [ActorResources(typeof(GenericBackgroundBehaviour), false)]
    public class GenericBackground : GenericActor<GenericBackgroundBehaviour, BackgroundMetadata>, IBackgroundActor
    {
        public GenericBackground (string id, BackgroundMetadata metadata)
            : base(id, metadata) { }

    }
}
