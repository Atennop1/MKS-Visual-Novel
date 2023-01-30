// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows rendering a texture with <see cref="TransitionalMaterial"/> and transition to another texture with a set of configurable visual effects.
    /// </summary>
    public abstract class TransitionalRenderer : MonoBehaviour
    {
        /// <summary>
        /// Current transition mode data.
        /// </summary>
        public virtual Transition Transition
        {
            get => new Transition(Material.TransitionName, Material.TransitionParams, Material.DissolveTexture);
            set
            {
                Material.TransitionName = value.Name;
                Material.TransitionParams = value.Parameters;
                Material.DissolveTexture = value.DissolveTexture;
            }
        }
        /// <inheritdoc cref="TransitionalMaterial.MainTexture"/>
        public virtual Texture MainTexture { get => Material.MainTexture; set => Material.MainTexture = value; }
        /// <inheritdoc cref="TransitionalMaterial.TransitionTexture"/>
        public virtual Texture TransitionTexture { get => Material.TransitionTexture; set => Material.TransitionTexture = value; }
        /// <inheritdoc cref="TransitionalMaterial.TransitionProgress"/>
        public virtual float TransitionProgress { get => Material.TransitionProgress; set => Material.TransitionProgress = value; }
        /// <inheritdoc cref="TransitionalMaterial.TintColor"/>
        public virtual Color TintColor { get => Material.TintColor; set => Material.TintColor = value; }
        /// <inheritdoc cref="TransitionalMaterial.Opacity"/>
        public virtual float Opacity { get => Material.Opacity; set => Material.Opacity = value; }
        /// <summary>
        /// Whether to flip the content by X-axis.
        /// </summary>
        public virtual bool FlipX { get; set; } = false;
        /// <summary>
        /// Whether to flip the content by Y-axis.
        /// </summary>
        public virtual bool FlipY { get; set; } = false;
        /// <summary>
        /// Intensity of the gaussian blur effect to apply for the rendered target.
        /// </summary>
        public virtual float BlurIntensity { get; set; } = 0f;
        /// <summary>
        /// Pivot of the textures inside render rectangle.
        /// </summary>
        public virtual Vector2 Pivot { get; set; } = new Vector2(.5f, .5f);

        /// <summary>
        /// Material used for rendering the content.
        /// </summary>
        protected virtual TransitionalMaterial Material { get; private set; }

        private readonly Tweener<FloatTween> transitionTweener = new Tweener<FloatTween>();
        private readonly Tweener<ColorTween> colorTweener = new Tweener<ColorTween>();
        private readonly Tweener<FloatTween> fadeTweener = new Tweener<FloatTween>();
        private readonly Tweener<FloatTween> blurTweener = new Tweener<FloatTween>();

        private BlurFilter blurFilter;
        private float opacityLastFrame;

        /// <summary>
        /// Adds a transitional renderer component for the provided actor.
        /// </summary>
        /// <param name="premultipliedAlpha">Whether the content has already been rendered and has RGB multiplied by opacity.</param>
        public static TransitionalRenderer CreateFor (OrthoActorMetadata actorMetadata, GameObject actorObject, bool premultipliedAlpha)
        {
            if (actorMetadata.RenderTexture)
            {
                actorMetadata.RenderTexture.Clear();
                var textureRenderer = actorObject.AddComponent<TransitionalTextureRenderer>();
                textureRenderer.Initialize(premultipliedAlpha, actorMetadata.CustomTextureShader);
                textureRenderer.RenderTexture = actorMetadata.RenderTexture;
                textureRenderer.RenderRectangle = actorMetadata.RenderRectangle;
                return textureRenderer;
            }
            else
            {
                var spriteRenderer = actorObject.AddComponent<TransitionalSpriteRenderer>();
                spriteRenderer.Initialize(actorMetadata.Pivot, actorMetadata.PixelsPerUnit, premultipliedAlpha,
                    actorMetadata.CustomTextureShader, actorMetadata.CustomSpriteShader);
                spriteRenderer.DepthPassEnabled = actorMetadata.EnableDepthPass;
                spriteRenderer.DepthAlphaCutoff = actorMetadata.DepthAlphaCutoff;
                return spriteRenderer;
            }
        }

        /// <summary>
        /// Performs transition from <see cref="TransitionalMaterial.MainTexture"/> to the provided <paramref name="texture"/> over <paramref name="duration"/>.
        /// </summary>
        /// <param name="texture">Texture to transition into.</param>
        /// <param name="duration">Duration of the transition, in seconds.</param>
        /// <param name="easingType">Type of easing to use when applying the transition effect.</param>
        /// <param name="transition">Type of the transition effect to use.</param>
        public virtual async UniTask TransitionToAsync (Texture texture, float duration, EasingType easingType = default,
            Transition? transition = default, AsyncToken asyncToken = default)
        {
            if (transitionTweener.Running)
            {
                transitionTweener.CompleteInstantly();
                await AsyncUtils.WaitEndOfFrameAsync(); // Materials are updated later in render loop, so wait before further modifications.
                asyncToken.ThrowIfCanceled(Material);
            }

            if (transition.HasValue)
                Transition = transition.Value;

            TransitionProgress = 0;
            
            if (duration <= 0)
            {
                MainTexture = texture;
                return;
            }

            if (!MainTexture) MainTexture = texture;
            TransitionTexture = texture;
            Material.UpdateRandomSeed();
            var tween = new FloatTween(0, 1, duration, value => TransitionProgress = value, false, easingType);
            await transitionTweener.RunAsync(tween, asyncToken, Material);
            MainTexture = texture;
            TransitionProgress = 0;
            TransitionTexture = null;
        }

        /// <summary>
        /// Tints current texture to the provided <paramref name="color"/> over <paramref name="duration"/>.
        /// </summary>
        /// <param name="color">Color of the tint.</param>
        /// <param name="duration">Duration of crossfade from current to the target tint color.</param>
        /// <param name="easingType">Type of easing to use when applying the tint.</param>
        public virtual async UniTask TintToAsync (Color color, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            if (colorTweener.Running) colorTweener.CompleteInstantly();

            if (duration <= 0)
            {
                TintColor = color;
                return;
            }

            var tween = new ColorTween(TintColor, color, ColorTweenMode.All, duration, value => TintColor = value, false, easingType);
            await colorTweener.RunAsync(tween, asyncToken, Material);
        }

        /// <summary>
        /// Same as tint, but applies only to the alpha component of the color.
        /// </summary>
        public virtual async UniTask FadeToAsync (float opacity, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            if (fadeTweener.Running) fadeTweener.CompleteInstantly();

            if (duration <= 0)
            {
                Opacity = opacity;
                return;
            }

            var tween = new FloatTween(Opacity, opacity, duration, value => Opacity = value, false, easingType);
            await fadeTweener.RunAsync(tween, asyncToken, Material);
        }

        public virtual async UniTask FadeOutAsync (float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            await FadeToAsync(0, duration, easingType, asyncToken);
        }

        public virtual async UniTask FadeInAsync (float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            await FadeToAsync(1, duration, easingType, asyncToken);
        }

        public virtual async UniTask BlurAsync (float intensity, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            if (blurTweener.Running) blurTweener.CompleteInstantly();

            if (duration <= 0)
            {
                BlurIntensity = intensity;
                return;
            }

            var tween = new FloatTween(BlurIntensity, intensity, duration, value => BlurIntensity = value, false, easingType);
            await blurTweener.RunAsync(tween, asyncToken, this);
        }

        public virtual CharacterLookDirection GetLookDirection (CharacterLookDirection bakedDirection)
        {
            switch (bakedDirection)
            {
                case CharacterLookDirection.Center:
                    return CharacterLookDirection.Center;
                case CharacterLookDirection.Left:
                    return FlipX ? CharacterLookDirection.Right : CharacterLookDirection.Left;
                case CharacterLookDirection.Right:
                    return FlipX ? CharacterLookDirection.Left : CharacterLookDirection.Right;
                default: return default;
            }
        }

        public virtual void SetLookDirection (CharacterLookDirection direction, CharacterLookDirection bakedDirection)
        {
            if (bakedDirection == CharacterLookDirection.Center) return;
            if (direction == CharacterLookDirection.Center)
            {
                FlipX = false;
                return;
            }
            if (direction != GetLookDirection(bakedDirection)) FlipX = !FlipX;
        }

        public virtual async UniTask ChangeLookDirectionAsync (CharacterLookDirection direction, CharacterLookDirection bakedDirection,
            float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            var prevValue = GetLookDirection(bakedDirection);
            SetLookDirection(direction, bakedDirection);
            if (prevValue != GetLookDirection(bakedDirection) && duration > 0)
                await FlipXAsync(duration, easingType, asyncToken);
        }

        /// <summary>
        /// Prepares the underlying systems for render.
        /// </summary>
        /// <param name="customShader">Shader to use for rendering; will use a default one when not provided.</param>
        /// <param name="premultipliedAlpha">Whether the content has already been rendered and has RGB multiplied by opacity.</param>
        protected virtual void Initialize (bool premultipliedAlpha, Shader customShader = default)
        {
            Material = new TransitionalMaterial(premultipliedAlpha, customShader);
            blurFilter = new BlurFilter(2, true);
        }

        protected virtual async UniTask FlipXAsync (float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            if (duration <= 0) return;
            Material.FlipMain = true;
            if (transitionTweener.Running)
                while (transitionTweener.Running && asyncToken.EnsureNotCanceledOrCompleted(this))
                    await AsyncUtils.WaitEndOfFrameAsync();
            else await TransitionToAsync(MainTexture, duration, easingType, null, asyncToken);
            Material.FlipMain = false;
        }

        protected virtual void OnDestroy ()
        {
            ObjectUtils.DestroyOrImmediate(Material);
            blurFilter?.Dispose();
        }

        protected virtual bool ShouldRender ()
        {
            return Material && MainTexture && opacityLastFrame > 0;
        }

        /// <summary>
        /// Renders <see cref="Material"/> to the provided render texture.
        /// </summary>
        /// <param name="texture">The render target.</param>
        protected virtual void RenderToTexture (RenderTexture texture)
        {
            var renderSize = new Vector2(texture.width, texture.height);
            FitUVs(renderSize);
            DrawQuad(texture, renderSize);
            if (BlurIntensity > 0) blurFilter.BlurTexture(texture, BlurIntensity);
        }

        protected virtual void FitUVs (Vector2 renderSize)
        {
            var mainSize = new Vector2(MainTexture.width, MainTexture.height);
            (Material.MainTextureOffset, Material.MainTextureScale) = GetUVModifiers(renderSize, mainSize);
            if (!TransitionTexture) return;
            var transitionSize = new Vector2(TransitionTexture.width, TransitionTexture.height);
            (Material.TransitionTextureOffset, Material.TransitionTextureScale) = GetUVModifiers(renderSize, transitionSize);
        }

        protected virtual (Vector2 offset, Vector2 scale) GetUVModifiers (Vector2 renderSize, Vector2 textureSize)
        {
            if (renderSize == textureSize) return (Vector2.zero, Vector2.one);
            if (renderSize.x < textureSize.x || renderSize.y < textureSize.y)
                renderSize /= textureSize.x > textureSize.y ? renderSize.x / textureSize.x : renderSize.y / textureSize.y;
            var offset = (textureSize - renderSize) / textureSize * Pivot;
            var scale = renderSize / textureSize;
            return (offset, scale);
        }

        protected virtual void DrawQuad (RenderTexture target, Vector2 size)
        {
            Graphics.SetRenderTarget(target);
            Material.SetPass(0);
            GL.Clear(true, true, Color.clear);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, size.x, 0, size.y);
            GL.Begin(GL.QUADS);
            GL.TexCoord2(FlipX ? 1 : 0, FlipY ? 1 : 0);
            GL.Vertex3(0, 0, 0);
            GL.TexCoord2(FlipX ? 0 : 1, FlipY ? 1 : 0);
            GL.Vertex3(size.x, 0, 0);
            GL.TexCoord2(FlipX ? 0 : 1, FlipY ? 0 : 1);
            GL.Vertex3(size.x, size.y, 0);
            GL.TexCoord2(FlipX ? 1 : 0, FlipY ? 0 : 1);
            GL.Vertex3(0, size.y, 0);
            GL.End();
            GL.PopMatrix();
        }

        private void LateUpdate () => opacityLastFrame = Opacity;
    }
}
