// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Can be applied to a command parameter to associate specified constant value range.
    /// Used by the bridging service to provide the context for external tools (IDE extension, web editor, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ConstantContextAttribute : ParameterContextAttribute
    {
        public readonly Type EnumType;

        /// <param name="enumType">An enum type to extract constant values from.</param>
        /// <param name="namedIndex">When applied to named parameter, specify index of the associated value (0 is for name and 1 for value).</param>
        public ConstantContextAttribute (Type enumType, int namedIndex = -1) : base(enumType.Name, namedIndex)
        {
            if (!enumType.IsEnum) throw new ArgumentException("Only enum types are supported.");
            EnumType = enumType;
        }
    }
}
