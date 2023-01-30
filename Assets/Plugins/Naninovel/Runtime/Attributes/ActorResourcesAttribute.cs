// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When applied to a <see cref="IActor"/> implementation, controls resources-related policies in editor menus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ActorResourcesAttribute : Attribute
    {
        public readonly Type TypeConstraint;
        public readonly bool AllowMultiple;

        /// <param name="typeConstraint">Type constraint for the resource field in editor menus. When null is provided, the resources editor won't be drawn.</param>
        /// <param name="allowMultiple">Whether to allow assigning multiple resources in the editor menus.</param>
        public ActorResourcesAttribute (Type typeConstraint, bool allowMultiple)
        {
            TypeConstraint = typeConstraint;
            AllowMultiple = allowMultiple;
        }
    }
}
