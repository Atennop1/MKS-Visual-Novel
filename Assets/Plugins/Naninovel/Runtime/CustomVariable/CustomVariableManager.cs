// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using Naninovel.Commands;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ICustomVariableManager"/>
    /// <remarks>Initialization order lowered, as other services implicitly use custom variables (eg, via <see cref="ExpressionEvaluator"/>).</remarks>
    [InitializeAtRuntime(int.MinValue + 1), Goto.DontReset]
    public class CustomVariableManager : IStatefulService<GameStateMap>, IStatefulService<GlobalStateMap>, ICustomVariableManager
    {
        [Serializable]
        public class GlobalState
        {
            public SerializableLiteralStringMap GlobalVariableMap;
        }

        [Serializable]
        public class GameState
        {
            public SerializableLiteralStringMap LocalVariableMap;
        }

        public event Action<CustomVariableUpdatedArgs> OnVariableUpdated;

        public virtual CustomVariablesConfiguration Configuration { get; }

        private readonly SerializableLiteralStringMap globalVariableMap;
        private readonly SerializableLiteralStringMap localVariableMap;

        public CustomVariableManager (CustomVariablesConfiguration config)
        {
            Configuration = config;
            globalVariableMap = new SerializableLiteralStringMap();
            localVariableMap = new SerializableLiteralStringMap();
        }

        public virtual UniTask InitializeServiceAsync () => UniTask.CompletedTask;

        public virtual void ResetService ()
        {
            ResetLocalVariables();
        }

        public virtual void DestroyService () { }

        public virtual void SaveServiceState (GlobalStateMap stateMap)
        {
            var state = new GlobalState {
                GlobalVariableMap = new SerializableLiteralStringMap(globalVariableMap)
            };
            stateMap.SetState(state);
        }

        public virtual UniTask LoadServiceStateAsync (GlobalStateMap stateMap)
        {
            ResetGlobalVariables();

            var state = stateMap.GetState<GlobalState>();
            if (state is null) return UniTask.CompletedTask;

            foreach (var kv in state.GlobalVariableMap)
                globalVariableMap[kv.Key] = kv.Value;
            return UniTask.CompletedTask;
        }

        public virtual void SaveServiceState (GameStateMap stateMap)
        {
            var state = new GameState {
                LocalVariableMap = new SerializableLiteralStringMap(localVariableMap)
            };
            stateMap.SetState(state);
        }

        public virtual UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            ResetLocalVariables();

            var state = stateMap.GetState<GameState>();
            if (state is null) return UniTask.CompletedTask;

            foreach (var kv in state.LocalVariableMap)
                localVariableMap[kv.Key] = kv.Value;
            return UniTask.CompletedTask;
        }

        public virtual bool VariableExists (string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
            return CustomVariablesConfiguration.IsGlobalVariable(name) ? globalVariableMap.ContainsKey(name) : localVariableMap.ContainsKey(name);
        }

        public virtual string GetVariableValue (string name)
        {
            if (!VariableExists(name)) return null;
            return CustomVariablesConfiguration.IsGlobalVariable(name) ? globalVariableMap[name] : localVariableMap[name];
        }

        public virtual IReadOnlyCollection<CustomVariable> GetAllVariables ()
        {
            var result = new List<CustomVariable>();
            foreach (var kv in globalVariableMap)
                result.Add(new CustomVariable(kv.Key, kv.Value));
            foreach (var kv in localVariableMap)
                result.Add(new CustomVariable(kv.Key, kv.Value));
            return result;
        }

        public virtual void SetVariableValue (string name, string value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be null or empty.", nameof(name));
            
            var isGlobal = CustomVariablesConfiguration.IsGlobalVariable(name);
            var initialValue = default(string);

            if (isGlobal)
            {
                globalVariableMap.TryGetValue(name, out initialValue);
                globalVariableMap[name] = value;
            }
            else
            {
                localVariableMap.TryGetValue(name, out initialValue);
                localVariableMap[name] = value;
            }

            if (initialValue != value)
                OnVariableUpdated?.Invoke(new CustomVariableUpdatedArgs(name, value, initialValue));
        }

        public virtual void ResetLocalVariables ()
        {
            localVariableMap?.Clear();

            foreach (var varData in Configuration.PredefinedVariables)
            {
                if (CustomVariablesConfiguration.IsGlobalVariable(varData.Name)) continue;
                var value = ExpressionEvaluator.Evaluate<string>(varData.Value, e => LogInitializeVarError(varData.Name, varData.Value, e));
                SetVariableValue(varData.Name, value);
            }
        }

        public virtual void ResetGlobalVariables ()
        {
            globalVariableMap?.Clear();

            foreach (var varData in Configuration.PredefinedVariables)
            {
                if (!CustomVariablesConfiguration.IsGlobalVariable(varData.Name)) continue;
                var value = ExpressionEvaluator.Evaluate<string>(varData.Value, e => LogInitializeVarError(varData.Name, varData.Value, e));
                SetVariableValue(varData.Name, value);
            }
        }

        private void LogInitializeVarError (string varName, string expr, string error) => Debug.LogWarning($"Failed to initialize `{varName}` variable with `{expr}` expression: {error}");
    }
}
