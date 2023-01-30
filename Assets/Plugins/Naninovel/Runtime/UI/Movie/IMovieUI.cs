// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to present movies played via <see cref="IMoviePlayer"/>.
    /// </summary>
    public interface IMovieUI : IManagedUI
    {
        /// <summary>
        /// Assigns texture to which the movies are rendered.
        /// </summary>
        void SetMovieTexture (Texture texture);
    }
}
