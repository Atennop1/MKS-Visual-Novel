// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="NamedInteger"/> value.
    /// </summary>
    [Serializable]
    public class NamedIntegerParameter : NamedParameter<NamedInteger, NullableInteger>
    {
        public static implicit operator NamedIntegerParameter (NamedInteger value) => new NamedIntegerParameter { Value = value };
        public static implicit operator NamedInteger (NamedIntegerParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NamedInteger ParseValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseIntegerText(namedValueText, out errors) as int?;
            return new NamedInteger(name, namedValue);
        }
    }
}
