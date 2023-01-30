// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#if SPRITE_DICING_AVAILABLE

using SpriteDicing;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using "SpriteDicing" extension to represent the actor.
    /// </summary>
    [ActorResources(typeof(DicedSpriteAtlas), false)]
    public class DicedSpriteBackground : DicedSpriteActor<BackgroundMetadata>, IBackgroundActor
    {
        private BackgroundMatcher matcher;
        
        public DicedSpriteBackground (string id, BackgroundMetadata metadata) 
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

#endif
