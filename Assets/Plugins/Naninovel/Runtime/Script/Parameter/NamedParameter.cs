// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="Named{TValue}"/> value.
    /// </summary>
    public abstract class NamedParameter<TValue, TNamedValue> : CommandParameter<TValue>
        where TValue : INamed<TNamedValue>
        where TNamedValue : class, INullableValue
    {
        /// <summary>
        /// Name component of the value or null when value is not assigned.
        /// </summary>
        public string Name => HasValue ? Value.Name : null;
        /// <summary>
        /// Value component of the value or null when value is not assigned.
        /// </summary>
        public TNamedValue NamedValue => HasValue ? Value.Value : null;

        public override string ToString () => base.ToString().Replace(".null", string.Empty).Replace("null", string.Empty);
    }
}