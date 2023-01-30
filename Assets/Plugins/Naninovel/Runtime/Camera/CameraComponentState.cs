// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represent serializable state of a component attached to <see cref="CameraManager.Camera"/>.
    /// </summary>
    [Serializable]
    public struct CameraComponentState : IEquatable<CameraComponentState>
    {
        public string TypeName => typeName;
        public bool Enabled => enabled;

        [SerializeField] private string typeName;
        [SerializeField] private bool enabled;
        
        public CameraComponentState (MonoBehaviour component)
        {
            typeName = component.GetType().Name;
            enabled = component.enabled;
        }
        
        public bool Equals (CameraComponentState other) => typeName == other.typeName;
        public override bool Equals (object obj) => obj is CameraComponentState other && Equals(other);
        public override int GetHashCode () => TypeName != null ? TypeName.GetHashCode() : 0;
        public static bool operator == (CameraComponentState left, CameraComponentState right) => left.Equals(right);
        public static bool operator != (CameraComponentState left, CameraComponentState right) => !left.Equals(right);
    }
}
