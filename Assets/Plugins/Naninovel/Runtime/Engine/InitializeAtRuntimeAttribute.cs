// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When applied to a <see cref="IEngineService"/> implementation,
    /// adds the service to the initialization list of <see cref="RuntimeInitializer"/>.
    /// </summary>
    /// <remarks>
    /// Requires the implementation to have either a default ctor, or a ctor with the following parameters (in any order):
    /// <see cref="IEngineService"/> objects with this attribute applied, <see cref="IEngineBehaviour"/> object or <see cref="Configuration"/> objects.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InitializeAtRuntimeAttribute : Attribute
    {
        public int InitializationPriority { get; }
        public Type Override { get; }

        /// <param name="initializationPriority">Moves the service in the initialization queue; lower the value earlier it'll be initialized.</param>
        /// <param name="override">Provide one of the built-in service types to override it.</param>
        public InitializeAtRuntimeAttribute (int initializationPriority = 0, Type @override = null)
        {
            InitializationPriority = initializationPriority;
            Override = @override;
            
            if (@override != null && @override.IsInterface) 
                throw new ArgumentException("To override a built-in engine service provide type of that service, not an interface.", nameof(@override));
        }
    }
}
