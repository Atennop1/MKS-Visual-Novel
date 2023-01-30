// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Describes a transition render effect.
    /// </summary>
    public readonly struct Transition : IEquatable<Transition>
    {
        /// <summary>
        /// Name of the transition.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Parameters of the transition.
        /// </summary>
        public Vector4 Parameters => parameters ?? TransitionUtils.GetDefaultParams(Name);
        /// <summary>
        /// Dissolve texture (mask) to use when <see cref="TransitionType.Custom"/>.
        /// </summary>
        public readonly Texture DissolveTexture;

        private readonly Vector4? parameters;

        /// <summary>
        /// Creates a new instance with the provided transition name (case-insensitive) and parameters.
        /// </summary>
        public Transition (string name, Vector4? parameters = default, Texture dissolveTexture = default)
        {
            Name = name;
            DissolveTexture = dissolveTexture;
            this.parameters = parameters;
        }
        
        public bool Equals (Transition other)
        {
            return Name == other.Name && Equals(DissolveTexture, other.DissolveTexture) && Nullable.Equals(parameters, other.parameters);
        }

        public override bool Equals (object obj)
        {
            return obj is Transition other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (DissolveTexture != null ? DissolveTexture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ parameters.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator == (Transition left, Transition right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Transition left, Transition right)
        {
            return !left.Equals(right);
        }
    }
}
