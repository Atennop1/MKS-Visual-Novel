// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableNamedFloat"/> values.
    /// </summary>
    [Serializable]
    public class NamedDecimalListParameter : ParameterList<NullableNamedFloat>
    {
        public static implicit operator NamedDecimalListParameter (List<NullableNamedFloat> value) => new NamedDecimalListParameter { Value = value };
        public static implicit operator List<NullableNamedFloat> (NamedDecimalListParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NullableNamedFloat ParseItemValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseFloatText(namedValueText, out errors) as float?;
            return new NamedFloat(name, namedValue);
        }
    }
}
