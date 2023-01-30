// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IConfigurationProvider"/>.
    /// </summary>
    public static class ConfigurationProviderExtensions
    {
        /// <summary>
        /// Attempts to provide a <see cref="Configuration"/> object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the requested configuration object.</typeparam>
        public static T GetConfiguration<T> (this IConfigurationProvider provider)
            where T : Configuration
        {
            return provider.GetConfiguration(typeof(T)) as T;
        }
    }
}
