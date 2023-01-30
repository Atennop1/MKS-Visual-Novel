// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="IActorManager{TActor, TState, TMeta, TConfig}"/>
    public abstract class ActorManager<TActor, TState, TMeta, TConfig> : IActorManager<TActor, TState, TMeta, TConfig>
        where TActor : IActor
        where TState : ActorState<TActor>, new()
        where TMeta : ActorMetadata
        where TConfig : ActorManagerConfiguration<TMeta>
    {
        [Serializable]
        public class GameState : ISerializationCallbackReceiver
        {
            public Dictionary<string, TState> ActorsMap { get; set; } = new Dictionary<string, TState>();

            [SerializeField] private SerializableLiteralStringMap actorsJsonMap = new SerializableLiteralStringMap();

            public virtual void OnBeforeSerialize ()
            {
                actorsJsonMap.Clear();
                foreach (var kv in ActorsMap)
                {
                    var stateJson = kv.Value.ToJson();
                    actorsJsonMap.Add(kv.Key, stateJson);
                }
            }

            public virtual void OnAfterDeserialize ()
            {
                ActorsMap.Clear();
                foreach (var kv in actorsJsonMap)
                {
                    var state = new TState();
                    state.OverwriteFromJson(kv.Value);
                    ActorsMap.Add(kv.Key, state);
                }
            }
        }

        public event Action<string> OnActorAdded;
        public event Action<string> OnActorRemoved;

        public TConfig Configuration { get; }
        public ActorManagerConfiguration ActorManagerConfiguration => Configuration;

        protected readonly Dictionary<string, TActor> ManagedActors;

        private static IReadOnlyCollection<Type> implementationTypes;

        private readonly Dictionary<string, UniTaskCompletionSource<TActor>> pendingAddActorTasks;

        protected ActorManager (TConfig config)
        {
            Configuration = config;
            ManagedActors = new Dictionary<string, TActor>(StringComparer.Ordinal);
            pendingAddActorTasks = new Dictionary<string, UniTaskCompletionSource<TActor>>();
        }

        public virtual UniTask InitializeServiceAsync ()
        {
            if (implementationTypes is null)
                implementationTypes = Engine.Types.Where(t => t.GetInterfaces().Contains(typeof(TActor))).ToArray();
            return UniTask.CompletedTask;
        }

        public virtual void ResetService ()
        {
            RemoveAllActors();
        }

        public virtual void DestroyService ()
        {
            RemoveAllActors();
        }

        public virtual void SaveServiceState (GameStateMap stateMap)
        {
            var state = new GameState();
            foreach (var kv in ManagedActors)
            {
                var actorState = new TState();
                actorState.OverwriteFromActor(kv.Value);
                state.ActorsMap.Add(kv.Key, actorState);
            }
            stateMap.SetState(state);
        }

        public virtual async UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.GetState<GameState>();
            if (state is null)
            {
                RemoveAllActors();
                return;
            }

            // Remove actors that doesn't exist in the serialized state.
            if (ManagedActors.Count > 0)
                foreach (var actorId in ManagedActors.Keys.ToList())
                    if (!state.ActorsMap.ContainsKey(actorId))
                        RemoveActor(actorId);

            foreach (var kv in state.ActorsMap)
            {
                var actor = await GetOrAddActorAsync(kv.Key);
                kv.Value.ApplyToActor(actor);
            }
        }

        public virtual bool ActorExists (string actorId) => !string.IsNullOrEmpty(actorId) && ManagedActors.ContainsKey(actorId);

        public virtual async UniTask<TActor> AddActorAsync (string actorId)
        {
            if (ActorExists(actorId))
            {
                Debug.LogWarning($"Actor '{actorId}' was requested to be added, but it already exists.");
                return GetActor(actorId);
            }

            if (pendingAddActorTasks.ContainsKey(actorId))
                return await pendingAddActorTasks[actorId].Task;

            pendingAddActorTasks[actorId] = new UniTaskCompletionSource<TActor>();

            var constructedActor = await ConstructActorAsync(actorId);
            ManagedActors.Add(actorId, constructedActor);

            pendingAddActorTasks[actorId].TrySetResult(constructedActor);
            pendingAddActorTasks.Remove(actorId);

            OnActorAdded?.Invoke(actorId);

            return constructedActor;
        }

        async UniTask<IActor> IActorManager.AddActorAsync (string actorId) => await AddActorAsync(actorId);

        public virtual async UniTask<TActor> AddActorAsync (string actorId, TState state)
        {
            if (string.IsNullOrWhiteSpace(actorId))
            {
                Debug.LogWarning($"Can't add an actor with '{state}' state: actor name is undefined.");
                return default;
            }

            var actor = await AddActorAsync(actorId);
            state.ApplyToActor(actor);
            return actor;
        }

        public virtual TActor GetActor (string actorId)
        {
            if (!ActorExists(actorId))
                throw new Exception($"Can't find '{actorId}' actor.");

            return ManagedActors[actorId];
        }

        IActor IActorManager.GetActor (string actorId) => GetActor(actorId);

        public virtual async UniTask<TActor> GetOrAddActorAsync (string actorId) => ActorExists(actorId) ? GetActor(actorId) : await AddActorAsync(actorId);

        public virtual IReadOnlyCollection<TActor> GetAllActors () => ManagedActors?.Values;

        IReadOnlyCollection<IActor> IActorManager.GetAllActors () => ManagedActors?.Values.Cast<IActor>().ToArray();

        public virtual void RemoveActor (string actorId)
        {
            if (!ActorExists(actorId)) return;

            var actor = GetActor(actorId);
            ManagedActors.Remove(actor.Id);
            (actor as IDisposable)?.Dispose();

            OnActorRemoved?.Invoke(actorId);
        }

        public virtual void RemoveAllActors ()
        {
            if (ManagedActors.Count == 0) return;
            var managedActors = GetAllActors().ToArray();
            for (int i = 0; i < managedActors.Length; i++)
                RemoveActor(managedActors[i].Id);
            ManagedActors.Clear();
        }

        ActorState IActorManager.GetActorState (string actorId) => GetActorState(actorId);

        public virtual TState GetActorState (string actorId)
        {
            if (!ActorExists(actorId))
                throw new Exception($"Can't retrieve state of a '{actorId}' actor: actor not found.");

            var actor = GetActor(actorId);
            var state = new TState();
            state.OverwriteFromActor(actor);
            return state;
        }

        protected virtual async UniTask<TActor> ConstructActorAsync (string actorId)
        {
            var metadata = Configuration.GetMetadataOrDefault(actorId);

            var implementationType = implementationTypes.FirstOrDefault(t => t.AssemblyQualifiedName == metadata.Implementation);
            if (implementationType is null) throw new Exception($"`{metadata.Implementation}` actor implementation type for `{typeof(TActor).Name}` is not found.");

            var actor = default(TActor);
            try { actor = (TActor)Activator.CreateInstance(implementationType, actorId, metadata); }
            catch { throw new Exception($"Failed to create instance of `{implementationType.FullName}` actor. Make sure the implementation has a compatible constructor."); }

            await actor.InitializeAsync();
            new TState().ApplyToActor(actor);

            return actor;
        }
    }
}
