// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a collection of <see cref="NullableBoolean"/> values.
    /// </summary>
    [Serializable]
    public class BooleanListParameter : ParameterList<NullableBoolean>
    {
        public static implicit operator BooleanListParameter (List<bool?> value) => new BooleanListParameter { Value = value?.Select(v => v.HasValue ? new NullableBoolean { Value = v.Value } : new NullableBoolean()).ToList() };
        public static implicit operator List<bool?> (BooleanListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (bool?)v.Value).ToList();
        public static implicit operator bool?[] (BooleanListParameter param) => param?.Value?.Select(v => v is null || !v.HasValue ? null : (bool?)v.Value).ToArray();

        protected override NullableBoolean ParseItemValueText (string valueText, out string errors) => ParseBooleanText(valueText, out errors);
    }
}
