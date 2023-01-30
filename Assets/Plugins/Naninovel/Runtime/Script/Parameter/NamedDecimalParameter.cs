// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="NamedFloat"/> value.
    /// </summary>
    [Serializable]
    public class NamedDecimalParameter : NamedParameter<NamedFloat, NullableFloat>
    {
        public static implicit operator NamedDecimalParameter (NamedFloat value) => new NamedDecimalParameter { Value = value };
        public static implicit operator NamedFloat (NamedDecimalParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NamedFloat ParseValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseFloatText(namedValueText, out errors) as float?;
            return new NamedFloat(name, namedValue);
        }
    }
}
