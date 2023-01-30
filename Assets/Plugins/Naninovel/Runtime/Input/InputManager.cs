// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Naninovel.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Naninovel
{
    /// <inheritdoc cref="IInputManager"/>
    [InitializeAtRuntime]
    public class InputManager : IStatefulService<GameStateMap>, IInputManager
    {
        [Serializable]
        public class GameState
        {
            public bool ProcessInput = true;
            public List<string> DisabledSamplers = default;
        }

        public virtual InputConfiguration Configuration { get; }
        public virtual bool ProcessInput { get; set; } = true;

        private readonly Dictionary<string, InputSampler> samplersMap = new Dictionary<string, InputSampler>(StringComparer.Ordinal);
        private readonly Dictionary<IManagedUI, string[]> blockingUIs = new Dictionary<IManagedUI, string[]>();
        private readonly HashSet<string> blockedSamplers = new HashSet<string>();
        private readonly CancellationTokenSource sampleCTS = new CancellationTokenSource();
        private GameObject gameObject;

        public InputManager (InputConfiguration config)
        {
            Configuration = config;
        }

        public virtual async UniTask InitializeServiceAsync ()
        {
            foreach (var binding in Configuration.Bindings)
            {
                var sampler = new InputSampler(Configuration, binding, null, this);
                samplersMap[binding.Name] = sampler;
            }

            gameObject = Engine.CreateObject(nameof(InputManager));

            if (Configuration.SpawnEventSystem)
            {
                if (ObjectUtils.IsValid(Configuration.CustomEventSystem))
                    Engine.Instantiate(Configuration.CustomEventSystem).transform.SetParent(gameObject.transform, false);
                else gameObject.AddComponent<EventSystem>();
            }

            if (Configuration.SpawnInputModule)
            {
                if (ObjectUtils.IsValid(Configuration.CustomInputModule))
                    Engine.Instantiate(Configuration.CustomInputModule).transform.SetParent(gameObject.transform, false);
                else
                {
                    #if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_AVAILABLE
                    var inputModule = gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    inputModule.AssignDefaultActions();
                    #else
                    var inputModule = gameObject.AddComponent<StandaloneInputModule>();
                    #endif
                    await AsyncUtils.WaitEndOfFrameAsync();
                    inputModule.enabled = false; // Otherwise it stops processing UI events when using new input system.
                    inputModule.enabled = true;
                }
            }

            SampleInputAsync(sampleCTS.Token).Forget();
        }

        public virtual void ResetService () { }

        public virtual void DestroyService ()
        {
            sampleCTS?.Cancel();
            sampleCTS?.Dispose();
            ObjectUtils.DestroyOrImmediate(gameObject);
        }

        public virtual void SaveServiceState (GameStateMap stateMap)
        {
            var state = new GameState {
                ProcessInput = ProcessInput,
                DisabledSamplers = samplersMap.Where(kv => !kv.Value.Enabled).Select(kv => kv.Key).ToList()
            };
            stateMap.SetState(state);
        }

        public virtual UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.GetState<GameState>();
            if (state is null) return UniTask.CompletedTask;

            ProcessInput = state.ProcessInput;

            foreach (var kv in samplersMap)
                kv.Value.Enabled = !state.DisabledSamplers?.Contains(kv.Key) ?? true;

            return UniTask.CompletedTask;
        }

        public virtual IInputSampler GetSampler (string bindingName)
        {
            if (!samplersMap.ContainsKey(bindingName)) return null;
            return samplersMap[bindingName];
        }

        public virtual void AddBlockingUI (IManagedUI ui, params string[] allowedSamplers)
        {
            if (blockingUIs.ContainsKey(ui)) return;
            blockingUIs.Add(ui, allowedSamplers);
            ui.OnVisibilityChanged += HandleBlockingUIVisibilityChanged;
            HandleBlockingUIVisibilityChanged(ui.Visible);
        }

        public virtual void RemoveBlockingUI (IManagedUI ui)
        {
            if (!blockingUIs.ContainsKey(ui)) return;
            blockingUIs.Remove(ui);
            ui.OnVisibilityChanged -= HandleBlockingUIVisibilityChanged;
            HandleBlockingUIVisibilityChanged(ui.Visible);
        }

        public bool IsSampling (string bindingName)
        {
            if (!ProcessInput) return false;
            if (!samplersMap.TryGetValue(bindingName, out var sampler)) return false;
            return sampler.Enabled && (!blockedSamplers.Contains(bindingName) || sampler.Binding.AlwaysProcess);
        }

        private void HandleBlockingUIVisibilityChanged (bool visible)
        {
            // If any of the blocking UIs are visible, all the samplers should be blocked,
            // except ones that are explicitly allowed by ALL the visible blocking UIs.

            // 1. Find the allowed samplers first; start with clearing the set.
            blockedSamplers.Clear();
            // 2. Store all the existing samplers.
            blockedSamplers.UnionWith(samplersMap.Keys);
            // 3. Remove samplers that are not allowed by any of the visible blocking UIs.
            foreach (var kv in blockingUIs)
                if (kv.Key.Visible)
                    blockedSamplers.IntersectWith(kv.Value);
            // 4. This will filter-out the samplers contained in both collections,
            // effectively storing only the non-allowed (blocked) ones in the set.
            blockedSamplers.SymmetricExceptWith(samplersMap.Keys);
        }

        private async UniTaskVoid SampleInputAsync (CancellationToken cancellationToken)
        {
            while (Application.isPlaying)
            {
                // It's important to sample early; eg, when sampling later and close button
                // of a blocking UI (eg, backlog) is pressed with enter key, the UI will un-block
                // before the sampling is performed, causing an unexpected continue input activation.
                await UniTask.Yield(PlayerLoopTiming.EarlyUpdate);
                if (cancellationToken.IsCancellationRequested) return;

                if (!ProcessInput) continue;

                foreach (var kv in samplersMap)
                    if (IsSampling(kv.Key))
                        kv.Value.SampleInput();
            }
        }
    }
}
