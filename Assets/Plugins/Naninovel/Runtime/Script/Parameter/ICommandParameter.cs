// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a <see cref="Command"/> parameter.
    /// </summary>
    public interface ICommandParameter
    {
        /// <summary>
        /// Whether a value is assigned.
        /// </summary>
        bool HasValue { get; }
        /// <summary>
        /// Whether the assigned value is dynamic (evaluated at runtime).
        /// </summary>
        bool DynamicValue { get; }

        /// <summary>
        /// Attempts to parse provided script text representing value
        /// of the parameter and set the result as the current value.
        /// </summary>
        /// <param name="valueText">Parameter value text to parse.</param>
        /// <param name="errors">Parse errors (if any) or null when the parse has succeeded.</param>
        void SetValue (string valueText, out string errors);
        /// <summary>
        /// Assigns a dynamic value to the parameter to be evaluated at runtime when accessed.
        /// </summary>
        void SetValue (DynamicValue dynamicValue);
    }
}
