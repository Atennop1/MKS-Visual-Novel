// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="NamedBoolean"/> value.
    /// </summary>
    [Serializable]
    public class NamedBooleanParameter : NamedParameter<NamedBoolean, NullableBoolean>
    {
        public static implicit operator NamedBooleanParameter (NamedBoolean value) => new NamedBooleanParameter { Value = value };
        public static implicit operator NamedBoolean (NamedBooleanParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NamedBoolean ParseValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseBooleanText(namedValueText, out errors) as bool?;
            return new NamedBoolean(name, namedValue);
        }
    }
}
