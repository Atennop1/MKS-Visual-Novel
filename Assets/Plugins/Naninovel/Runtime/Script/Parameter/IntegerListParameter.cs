// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableInteger"/> values.
    /// </summary>
    [Serializable]
    public class IntegerListParameter : ParameterList<NullableInteger>
    {
        public static implicit operator IntegerListParameter (List<int?> value) => new IntegerListParameter { Value = value?.Select(v => v.HasValue ? new NullableInteger { Value = v.Value } : new NullableInteger()).ToList() };
        public static implicit operator List<int?> (IntegerListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (int?)v.Value).ToList();
        public static implicit operator int?[] (IntegerListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (int?)v.Value).ToArray();

        protected override NullableInteger ParseItemValueText (string valueText, out string errors) => ParseIntegerText(valueText, out errors);
    }
}
