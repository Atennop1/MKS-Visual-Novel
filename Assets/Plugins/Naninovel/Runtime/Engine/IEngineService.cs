// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Implementation represents an <see cref="Engine"/> service.
    /// </summary>
    public interface IEngineService 
    {
        /// <summary>
        /// Initializes the service. When using other services during initialization, 
        /// specify references to them in constructor to make sure they're initialized at this point. 
        /// </summary>
        UniTask InitializeServiceAsync ();
        /// <summary>
        /// Resets the service state (game session-specific).
        /// </summary>
        void ResetService ();
        /// <summary>
        /// Stops the service, releases all the held resources and remove it from memory.
        /// </summary>
        void DestroyService ();
    }

    /// <summary>
    /// Implementation represents an <see cref="Engine"/> service with an associated <see cref="Naninovel.Configuration"/>.
    /// </summary>
    public interface IEngineService<TConfig> : IEngineService 
        where TConfig : Configuration
    {
        /// <summary>
        /// Configuration of the service.
        /// </summary>
        TConfig Configuration { get; }
    }
}
