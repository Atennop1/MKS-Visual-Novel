// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableNamedBoolean"/> values.
    /// </summary>
    [Serializable]
    public class NamedBooleanListParameter : ParameterList<NullableNamedBoolean>
    {
        public static implicit operator NamedBooleanListParameter (List<NullableNamedBoolean> value) => new NamedBooleanListParameter { Value = value };
        public static implicit operator List<NullableNamedBoolean> (NamedBooleanListParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NullableNamedBoolean ParseItemValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseBooleanText(namedValueText, out errors) as bool?;
            return new NamedBoolean(name, namedValue);
        }
    }
}
