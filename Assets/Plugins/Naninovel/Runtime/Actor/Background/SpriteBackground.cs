// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="SpriteActor{TMeta}"/> to represent the actor.
    /// </summary>
    [ActorResources(typeof(Texture2D), true)]
    public class SpriteBackground : SpriteActor<BackgroundMetadata>, IBackgroundActor
    {
        private BackgroundMatcher matcher;
        
        public SpriteBackground (string id, BackgroundMetadata metadata) 
            : base(id, metadata) { }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();
            matcher = BackgroundMatcher.CreateFor(ActorMetadata, TransitionalRenderer);
        }

        public override void Dispose ()
        {
            base.Dispose();
            matcher?.Stop();
        }
    } 
}
