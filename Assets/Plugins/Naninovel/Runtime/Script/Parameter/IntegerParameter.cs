// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a nullable <see cref="int"/> value.
    /// </summary>
    [Serializable]
    public class IntegerParameter : CommandParameter<int>
    {
        public static implicit operator IntegerParameter (int value) => new IntegerParameter { Value = value };
        public static implicit operator int? (IntegerParameter param) => param is null || !param.HasValue ? null : (int?)param.Value;
        public static implicit operator IntegerParameter (NullableInteger value) => new IntegerParameter { Value = value };
        public static implicit operator NullableInteger (IntegerParameter param) => param?.Value;

        protected override int ParseValueText (string valueText, out string errors) => ParseIntegerText(valueText, out errors);
    }
}
