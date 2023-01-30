// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    /// <inheritdoc cref="IMovieUI"/>
    public class MovieUI : CustomUI, IMovieUI
    {
        protected virtual RawImage MovieImage => movieImage;
        protected virtual RawImage FadeImage => fadeImage;

        [SerializeField] private RawImage movieImage = default;
        [SerializeField] private RawImage fadeImage = default;

        public virtual void SetMovieTexture (Texture texture)
        {
            MovieImage.texture = texture;
            MovieImage.SetOpacity(1);
        }

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(MovieImage, FadeImage);

            var config = Engine.GetConfiguration<MoviesConfiguration>();
            if (config.CustomFadeTexture)
                fadeImage.texture = config.CustomFadeTexture;
        }

        protected override void HandleVisibilityChanged (bool visible)
        {
            base.HandleVisibilityChanged(visible);

            MovieImage.texture = null;
            MovieImage.SetOpacity(0);
        }
    }
}
