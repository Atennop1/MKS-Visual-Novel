// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represent serializable state of <see cref="CameraLookController"/>.
    /// </summary>
    [Serializable]
    public struct CameraLookState : IEquatable<CameraLookState>
    {
        public bool Enabled => enabled;
        public bool Gravity => gravity;
        public Vector2 Zone => zone;
        public Vector2 Speed => speed;

        [SerializeField] private bool enabled;
        [SerializeField] private bool gravity;
        [SerializeField] private Vector2 zone;
        [SerializeField] private Vector2 speed;

        public CameraLookState (bool enabled, bool gravity, Vector2 zone, Vector2 speed)
        {
            this.enabled = enabled;
            this.gravity = gravity;
            this.zone = zone;
            this.speed = speed;
        }
        
        public bool Equals (CameraLookState other)
        {
            return enabled == other.enabled && gravity == other.gravity && zone.Equals(other.zone) && speed.Equals(other.speed);
        }

        public override bool Equals (object obj)
        {
            return obj is CameraLookState other && Equals(other);
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = Enabled.GetHashCode();
                hashCode = (hashCode * 397) ^ Gravity.GetHashCode();
                hashCode = (hashCode * 397) ^ Zone.GetHashCode();
                hashCode = (hashCode * 397) ^ Speed.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator == (CameraLookState left, CameraLookState right)
        {
            return left.Equals(right);
        }

        public static bool operator != (CameraLookState left, CameraLookState right)
        {
            return !left.Equals(right);
        }
    }
}
