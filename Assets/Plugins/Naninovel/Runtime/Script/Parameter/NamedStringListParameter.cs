// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableNamedString"/> values.
    /// </summary>
    [Serializable]
    public class NamedStringListParameter : ParameterList<NullableNamedString>
    {
        public static implicit operator NamedStringListParameter (List<NullableNamedString> value) => new NamedStringListParameter { Value = value };
        public static implicit operator List<NullableNamedString> (NamedStringListParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NullableNamedString ParseItemValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseStringText(namedValueText, out errors);
            return new NamedString(name, namedValue);
        }
    }
}
