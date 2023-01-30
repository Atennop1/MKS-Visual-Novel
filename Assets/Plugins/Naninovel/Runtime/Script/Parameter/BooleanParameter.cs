// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with a nullable <see cref="bool"/> value.
    /// </summary>
    [Serializable]
    public class BooleanParameter : CommandParameter<bool>
    {
        public static implicit operator BooleanParameter (bool value) => new BooleanParameter { Value = value };
        public static implicit operator bool? (BooleanParameter param) => param is null || !param.HasValue ? null : (bool?)param.Value;
        public static implicit operator BooleanParameter (NullableBoolean value) => new BooleanParameter { Value = value };
        public static implicit operator NullableBoolean (BooleanParameter param) => param?.Value;

        protected override bool ParseValueText (string valueText, out string errors) => ParseBooleanText(valueText, out errors);
    }
}
