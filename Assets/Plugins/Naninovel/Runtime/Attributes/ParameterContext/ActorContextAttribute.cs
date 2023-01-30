// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Can be applied to a command parameter to associate actor records with a specific path prefix.
    /// Used by the bridging service to provide the context for external tools (IDE extension, web editor, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ActorContextAttribute : ParameterContextAttribute
    {
        /// <param name="pathPrefix">Actor path prefix to associate with the parameter. When *, will associate with all the available actors.</param>
        /// <param name="namedIndex">When applied to named parameter, specify index of the associated value (0 is for name and 1 for value).</param>
        public ActorContextAttribute (string pathPrefix = "*", int namedIndex = -1) : base(pathPrefix, namedIndex) { }
    }
}
