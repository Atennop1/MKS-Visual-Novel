// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Can be applied to a command parameter to associate appearance records. Command should contains a parameter with <see cref="ActorContextAttribute"/>.
    /// Used by the bridging service to provide the context for external tools (IDE extension, web editor, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class AppearanceContextAttribute : ParameterContextAttribute
    {
        /// <param name="namedIndex">When applied to named parameter, specify index of the associated value (0 is for name and 1 for value).</param>
        public AppearanceContextAttribute (int namedIndex = -1) : base("", namedIndex) { }
    }
}
