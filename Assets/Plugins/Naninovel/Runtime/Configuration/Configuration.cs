// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Base class for project-specific configuration assets used to initialize and configure services and other engine systems.
    /// Serialized configuration assets are generated automatically under the engine's data folder and can be edited via Unity project settings menu
    /// when the implementation has an <see cref="EditInProjectSettingsAttribute"/> applied.
    /// </summary>
    public abstract class Configuration : ScriptableObject
    {
        /// <summary>
        /// When applied to a <see cref="Configuration"/> implementation, adds an associated editor settings menu.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class EditInProjectSettingsAttribute : Attribute { }

        /// <summary>
        /// Providers configuration of the requested type either via configuration provider
        /// (when engine is initialized) or by loading the asset from default resources folder.
        /// </summary>
        public static T GetOrDefault<T> () where T : Configuration
        {
            if (Engine.Initialized) return Engine.GetConfiguration<T>();
            else return ProjectConfigurationProvider.LoadOrDefault<T>();
        }
    }
}
