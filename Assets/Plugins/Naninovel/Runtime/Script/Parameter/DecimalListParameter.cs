// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableFloat"/> values.
    /// </summary>
    [Serializable]
    public class DecimalListParameter : ParameterList<NullableFloat>
    {
        public static implicit operator DecimalListParameter (List<float?> value) => new DecimalListParameter { Value = value?.Select(v => v.HasValue ? new NullableFloat { Value = v.Value } : new NullableFloat()).ToList() };
        public static implicit operator List<float?> (DecimalListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (float?)v.Value).ToList();
        public static implicit operator float?[] (DecimalListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (float?)v.Value).ToArray();

        protected override NullableFloat ParseItemValueText (string valueText, out string errors) => ParseFloatText(valueText, out errors);
    }
}
