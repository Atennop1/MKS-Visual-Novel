// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="NamedString"/> value.
    /// </summary>
    [Serializable]
    public class NamedStringParameter : NamedParameter<NamedString, NullableString>
    {
        public static implicit operator NamedStringParameter (NamedString value) => new NamedStringParameter { Value = value };
        public static implicit operator NamedString (NamedStringParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NamedString ParseValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseStringText(namedValueText, out errors);
            return new NamedString(name, namedValue);
        }
    }
}
