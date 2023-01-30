// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to handle movie playing.
    /// </summary>
    public interface IMoviePlayer : IEngineService<MoviesConfiguration>
    {
        /// <summary>
        /// Event invoked when playback is started.
        /// </summary>
        event Action OnMoviePlay;
        /// <summary>
        /// Event invoked when playback is stopped.
        /// </summary>
        event Action OnMovieStop;

        /// <summary>
        /// Whether currently playing or preparing to play a movie.
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// Starts playing a movie with the provided name.
        /// Returns texture to which the movie is rendered.
        /// </summary>
        UniTask<Texture> PlayAsync (string movieName, AsyncToken asyncToken = default);
        /// <summary>
        /// Stops the playback.
        /// </summary>
        void Stop ();
        /// <summary>
        /// Preloads the resources required to play a movie with the provided path.
        /// </summary>
        UniTask HoldResourcesAsync (string movieName, object holder);
        /// <summary>
        /// Unloads the resources required to play a movie with the provided path.
        /// </summary>
        void ReleaseResources (string movieName, object holder);
    }
}
