// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to provide <see cref="Configuration"/> objects.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Provides configuration object of the specified type.
        /// </summary>
        /// <param name="type">Type of the requested configuration object.</param>
        /// <returns>Configuration object of the requested type.</returns>
        Configuration GetConfiguration (Type type);
    }
}
