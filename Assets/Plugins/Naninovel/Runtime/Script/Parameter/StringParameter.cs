// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Represents a serializable <see cref="Command"/> parameter with <see cref="string"/> value.
    /// </summary>
    [Serializable]
    public class StringParameter : CommandParameter<string>
    {
        public static implicit operator StringParameter (string value) => new StringParameter { Value = value };
        public static implicit operator string (StringParameter param) => param is null || !param.HasValue ? null : param.Value;
        public static implicit operator StringParameter (NullableString value) => new StringParameter { Value = value };
        public static implicit operator NullableString (StringParameter param) => param?.Value;

        protected override string ParseValueText (string valueText, out string errors) => ParseStringText(valueText, out errors);
    }
}
