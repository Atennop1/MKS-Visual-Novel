// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a UI managed by <see cref="UIManager"/>.
    /// </summary>
    public readonly struct ManagedUI : IEquatable<ManagedUI>
    {
        public readonly string Name;
        public readonly GameObject GameObject;
        public readonly IManagedUI UIComponent;
        public readonly Type ComponentType;

        public ManagedUI ([NotNull] string name, [NotNull] GameObject gameObject, [NotNull] IManagedUI uiComponent)
        {
            Name = name;
            GameObject = gameObject;
            UIComponent = uiComponent;
            ComponentType = UIComponent.GetType();
        }
        
        public bool Equals (ManagedUI other) => Equals(UIComponent, other.UIComponent);
        public override bool Equals (object obj) => obj is ManagedUI other && Equals(other);
        public override int GetHashCode () => UIComponent != null ? UIComponent.GetHashCode() : 0;
        public static bool operator == (ManagedUI left, ManagedUI right) => left.Equals(right);
        public static bool operator != (ManagedUI left, ManagedUI right) => !left.Equals(right);
    }
}
