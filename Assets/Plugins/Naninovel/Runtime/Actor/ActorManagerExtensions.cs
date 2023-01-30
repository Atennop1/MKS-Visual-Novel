// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IActorManager"/>.
    /// </summary>
    public static class ActorManagerExtensions
    {
        /// <summary>
        /// Returns a managed actor with the provided ID. If the actor doesn't exist, will add it.
        /// </summary>
        public static async UniTask<IActor> GetOrAddActorAsync (this IActorManager manager, string actorId)
        {
            return manager.ActorExists(actorId) ? manager.GetActor(actorId) : await manager.AddActorAsync(actorId);
        }

        /// <summary>
        /// Returns a managed actor with the provided ID. If the actor doesn't exist, will add it.
        /// </summary>
        public static async UniTask<TActor> GetOrAddActorAsync<TActor, TState, TMeta, TConfig> (this IActorManager<TActor, TState, TMeta, TConfig> manager, string actorId)
            where TActor : IActor
            where TState : ActorState<TActor>, new()
            where TMeta : ActorMetadata
            where TConfig : ActorManagerConfiguration<TMeta>
        {
            return manager.ActorExists(actorId) ? manager.GetActor(actorId) : await manager.AddActorAsync(actorId);
        }
    }
}
