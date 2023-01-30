// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to evaluate dynamic value of a <see cref="ICommandParameter"/>.
    /// </summary>
    [Serializable]
    public class DynamicValue
    {
        public PlaybackSpot PlaybackSpot = default;
        public string ValueText = default;
        public string[] Expressions = default;

        public DynamicValue () { }

        public DynamicValue (PlaybackSpot playbackSpot, string valueText, IEnumerable<string> expressions)
        {
            PlaybackSpot = playbackSpot;
            ValueText = valueText;
            Expressions = expressions.ToArray();
        }
    }
}