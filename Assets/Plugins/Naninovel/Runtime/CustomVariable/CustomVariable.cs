// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a custom variable.
    /// </summary>
    [Serializable]
    public struct CustomVariable : IEquatable<CustomVariable>
    {
        /// <summary>
        /// Name of the custom variable.
        /// </summary>
        public string Name => name;
        /// <summary>
        /// Value of the custom variable.
        /// </summary>
        public string Value => value;
        /// <summary>
        /// Whether the variable is global variable.
        /// </summary>
        public bool Global => CustomVariablesConfiguration.IsGlobalVariable(Name);

        [SerializeField] private string name;
        [SerializeField] private string value;

        public CustomVariable (string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        
        public override bool Equals (object obj)
        {
            return obj is CustomVariable variable && Equals(variable);
        }

        public bool Equals (CustomVariable other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode ()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public static bool operator == (CustomVariable left, CustomVariable right)
        {
            return left.Equals(right);
        }

        public static bool operator != (CustomVariable left, CustomVariable right)
        {
            return !(left == right);
        }
    }
}
