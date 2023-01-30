// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Naninovel
{
    [Serializable]
    public struct ScriptGraphNodeState : IEquatable<ScriptGraphNodeState>
    {
        public string ScriptName => scriptName;
        public Rect Position => position;

        [FormerlySerializedAs("ScriptName")]
        [SerializeField] private string scriptName;
        [FormerlySerializedAs("Position")]
        [SerializeField] private Rect position;

        public ScriptGraphNodeState (string scriptName, Rect position)
        {
            this.scriptName = scriptName;
            this.position = position;
        }
        
        public bool Equals (ScriptGraphNodeState other)
        {
            return scriptName == other.scriptName && position.Equals(other.position);
        }

        public override bool Equals (object obj)
        {
            return obj is ScriptGraphNodeState other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                return ((ScriptName != null ? ScriptName.GetHashCode() : 0) * 397) ^ Position.GetHashCode();
            }
        }

        public static bool operator == (ScriptGraphNodeState left, ScriptGraphNodeState right)
        {
            return left.Equals(right);
        }

        public static bool operator != (ScriptGraphNodeState left, ScriptGraphNodeState right)
        {
            return !left.Equals(right);
        }
    }
}
