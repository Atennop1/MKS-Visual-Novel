// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the <see cref="ICustomVariableManager.OnVariableUpdated"/> event. 
    /// </summary>
    public class CustomVariableUpdatedArgs : EventArgs
    {
        /// <summary>
        /// Name of the updated variable.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// New value of the updated variable.
        /// </summary>
        public readonly string Value;
        /// <summary>
        /// Value the variable had before the update.
        /// </summary>
        public readonly string InitialValue;

        public CustomVariableUpdatedArgs (string name, string value, string initialValue)
        {
            Name = name;
            Value = value;
            InitialValue = initialValue;
        }
    }
}
