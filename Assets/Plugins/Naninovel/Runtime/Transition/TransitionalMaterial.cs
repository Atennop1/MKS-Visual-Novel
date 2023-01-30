// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    public class TransitionalMaterial : Material
    {
        /// <summary>
        /// Current main texture.
        /// </summary>
        public Texture MainTexture { get => mainTexture; set => mainTexture = value; }
        /// <summary>
        /// UV offset of the main texture.
        /// </summary>
        public virtual Vector2 MainTextureOffset { get => mainTextureOffset; set => mainTextureOffset = value; }
        /// <summary>
        /// UV scale of the main texture.
        /// </summary>
        public virtual Vector2 MainTextureScale { get => mainTextureScale; set => mainTextureScale = value; }
        /// <summary>
        /// Current texture that is used to transition from <see cref="MainTexture"/>.
        /// </summary>
        public Texture TransitionTexture { get => GetTexture(transitionTexId); set => SetTexture(transitionTexId, value); }
        /// <summary>
        /// UV offset of the transition texture.
        /// </summary>
        public virtual Vector2 TransitionTextureOffset { get => GetTextureOffset(transitionTexId); set => SetTextureOffset(transitionTexId, value); }
        /// <summary>
        /// UV scale of the transition texture.
        /// </summary>
        public virtual Vector2 TransitionTextureScale { get => GetTextureScale(transitionTexId); set => SetTextureScale(transitionTexId, value); }
        /// <summary>
        /// Texture used in a custom dissolve transition type.
        /// </summary>
        public Texture DissolveTexture { get => GetTexture(dissolveTexId); set => SetTexture(dissolveTexId, value); }
        /// <summary>
        /// Name of the current transition type.
        /// </summary>
        public string TransitionName { get => TransitionUtils.GetEnabled(this); set => TransitionUtils.EnableKeyword(this, value); }
        /// <summary>
        /// Current transition progress between <see cref="MainTexture"/> and <see cref="TransitionTexture"/>, in 0.0 to 1.0 range.
        /// </summary>
        public float TransitionProgress { get => GetFloat(transitionProgressId); set => SetFloat(transitionProgressId, value); }
        /// <summary>
        /// Parameters of the current transition.
        /// </summary>
        public Vector4 TransitionParams { get => GetVector(transitionParamsId); set => SetVector(transitionParamsId, value); }
        /// <summary>
        /// Current random seed used in some transition types.
        /// </summary>
        public Vector2 RandomSeed { get => GetVector(randomSeedId); set => SetVector(randomSeedId, value); }
        /// <summary>
        /// Current tint color.
        /// </summary>
        public Color TintColor { get => GetColor(tintColorId); set => SetColor(tintColorId, value); }
        /// <summary>
        /// Current alpha component of <see cref="TintColor"/>.
        /// </summary>
        public float Opacity { get => GetColor(tintColorId).a; set => SetOpacity(value); }
        /// <summary>
        /// Whether main texture is flipped by X-axis.
        /// </summary>
        public bool FlipMain { get => Mathf.Approximately(GetFloat(flipMainId), 1); set => SetFloat(flipMainId, value ? 1 : 0); }

        private const string defaultShaderName = "Naninovel/TransitionalTexture";
        private const string cloudsTexturePath = "Textures/Clouds";
        private const string premultipliedAlphaKey = "PREMULTIPLIED_ALPHA";
        private static readonly int mainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int transitionTexId = Shader.PropertyToID("_TransitionTex");
        private static readonly int cloudsTexId = Shader.PropertyToID("_CloudsTex");
        private static readonly int dissolveTexId = Shader.PropertyToID("_DissolveTex");
        private static readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");
        private static readonly int transitionParamsId = Shader.PropertyToID("_TransitionParams");
        private static readonly int randomSeedId = Shader.PropertyToID("_RandomSeed");
        private static readonly int tintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int flipMainId = Shader.PropertyToID("_FlipMainX");

        private static Texture2D sharedCloudsTexture;

        public TransitionalMaterial (bool premultipliedAlpha, Shader customShader = default, HideFlags hideFlags = HideFlags.HideAndDontSave)
            : base(customShader ? customShader : Shader.Find(defaultShaderName))
        {
            if (!sharedCloudsTexture)
                sharedCloudsTexture = Engine.LoadInternalResource<Texture2D>(cloudsTexturePath);
            if (premultipliedAlpha)
                EnableKeyword(premultipliedAlphaKey);
            SetTexture(cloudsTexId, sharedCloudsTexture);
            this.hideFlags = hideFlags;
        }

        /// <summary>
        /// Regenerate current value of <see cref="RandomSeed"/>.
        /// </summary>
        public void UpdateRandomSeed ()
        {
            var sinTime = Mathf.Sin(Time.time);
            var cosTime = Mathf.Cos(Time.time);
            RandomSeed = new Vector2(Mathf.Abs(sinTime), Mathf.Abs(cosTime));
        }

        private void SetOpacity (float value)
        {
            var color = TintColor;
            color.a = value;
            TintColor = color;
        }
    }
}
