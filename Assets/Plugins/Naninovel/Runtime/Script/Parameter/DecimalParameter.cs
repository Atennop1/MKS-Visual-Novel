// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a nullable <see cref="float"/> value.
    /// </summary>
    [Serializable]
    public class DecimalParameter : CommandParameter<float>
    {
        public static implicit operator DecimalParameter (float value) => new DecimalParameter { Value = value };
        public static implicit operator float? (DecimalParameter param) => param is null || !param.HasValue ? null : (float?)param.Value;
        public static implicit operator DecimalParameter (NullableFloat value) => new DecimalParameter { Value = value };
        public static implicit operator NullableFloat (DecimalParameter param) => param?.Value;

        protected override float ParseValueText (string valueText, out string errors) => ParseFloatText(valueText, out errors);
    }
}
