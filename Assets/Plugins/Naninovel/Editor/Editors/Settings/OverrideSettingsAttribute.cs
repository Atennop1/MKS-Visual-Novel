// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When applied to a <see cref="ConfigurationSettings"/> implementation,
    /// the implementation will be used instead of the built-in one when drawing Naninovel configuration editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OverrideSettingsAttribute : Attribute { }
}
