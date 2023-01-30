// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Naninovel.Commands;
using UnityEngine;

namespace Naninovel.FX
{
    public class Blur : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable, DestroySpawned.IParameterized, DestroySpawned.IAwaitable
    {
        public interface IBlurable
        {
            UniTask BlurAsync (float intensity, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
        }

        protected string ActorId { get; private set; }
        protected float Intensity { get; private set; }
        protected float Duration { get; private set; }
        protected float StopDuration { get; private set; }

        [SerializeField] private string defaultActorId = "MainBackground";
        [SerializeField] private float defaultIntensity = .5f;
        [SerializeField] private float defaultDuration = 1f;

        public virtual void SetSpawnParameters (IReadOnlyList<string> parameters)
        {
            ActorId = parameters?.ElementAtOrDefault(0) ?? defaultActorId;
            Intensity = Mathf.Abs(parameters?.ElementAtOrDefault(1)?.AsInvariantFloat() ?? defaultIntensity);
            Duration = Mathf.Abs(parameters?.ElementAtOrDefault(2)?.AsInvariantFloat() ?? defaultDuration);
        }

        public async UniTask AwaitSpawnAsync (AsyncToken asyncToken = default)
        {
            var actor = FindActor(ActorId);
            if (actor is null) return;
            var duration = asyncToken.Completed ? 0 : Duration;
            await actor.BlurAsync(Intensity, duration, EasingType.SmoothStep, asyncToken);
        }

        public void SetDestroyParameters (IReadOnlyList<string> parameters)
        {
            StopDuration = Mathf.Abs(parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultDuration);
        }

        public async UniTask AwaitDestroyAsync (AsyncToken asyncToken = default)
        {
            var actor = FindActor(ActorId);
            if (actor is null) return;
            var duration = asyncToken.Completed ? 0 : StopDuration;
            await actor.BlurAsync(0, duration, EasingType.SmoothStep, asyncToken);
        }

        private void OnDestroy () // Required to disable the effect on rollback.
        {
            FindActor(ActorId, false)?.BlurAsync(0, 0);
        }

        private static IBlurable FindActor (string actorId, bool logError = true)
        {
            var manager = Engine.FindAllServices<IActorManager>(c => c.ActorExists(actorId)).FirstOrDefault();
            if (manager is null)
            {
                if (logError) Debug.LogError($"Failed to apply blur effect: Can't find `{actorId}` actor");
                return null;
            }
            var blurable = manager.GetActor(actorId) as IBlurable;
            if (blurable is null)
            {
                if (logError) Debug.LogError($"Failed to apply blur effect: `{actorId}` actor doesn't support blur effect.");
                return null;
            }
            return blurable;
        }
    }
}
