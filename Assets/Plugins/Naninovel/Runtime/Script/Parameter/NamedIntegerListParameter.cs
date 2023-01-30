// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableNamedInteger"/> values.
    /// </summary>
    [Serializable]
    public class NamedIntegerListParameter : ParameterList<NullableNamedInteger>
    {
        public static implicit operator NamedIntegerListParameter (List<NullableNamedInteger> value) => new NamedIntegerListParameter { Value = value };
        public static implicit operator List<NullableNamedInteger> (NamedIntegerListParameter param) => param is null || !param.HasValue ? null : param.Value;

        protected override NullableNamedInteger ParseItemValueText (string valueText, out string errors)
        {
            ParseNamedValueText(valueText, out var name, out var namedValueText, out errors);
            var namedValue = string.IsNullOrEmpty(namedValueText) ? null : ParseIntegerText(namedValueText, out errors) as int?;
            return new NamedInteger(name, namedValue);
        }
    }
}
