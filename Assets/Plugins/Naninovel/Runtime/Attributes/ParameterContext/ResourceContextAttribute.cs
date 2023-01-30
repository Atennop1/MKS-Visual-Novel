// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Can be applied to a command parameter to associate resources with a specific path prefix.
    /// Used by the bridging service to provide the context for external tools (IDE extension, web editor, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ResourceContextAttribute : ParameterContextAttribute
    {
        /// <param name="pathPrefix">Resource path prefix to associate with the parameter.</param>
        /// <param name="namedIndex">When applied to named parameter, specify index of the associated value (0 is for name and 1 for value).</param>
        public ResourceContextAttribute (string pathPrefix, int namedIndex = -1) : base(pathPrefix, namedIndex) { }
    }
}
