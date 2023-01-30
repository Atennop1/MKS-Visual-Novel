// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    public abstract class ParameterContextAttribute : Attribute
    {
        public readonly string SubType;
        public readonly int NamedIndex;

        protected ParameterContextAttribute (string subType, int namedIndex = -1)
        {
            SubType = subType;
            NamedIndex = namedIndex;
        }
    }
}
