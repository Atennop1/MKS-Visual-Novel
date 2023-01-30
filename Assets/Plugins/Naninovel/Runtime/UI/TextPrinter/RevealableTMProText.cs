// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class RevealableTMProText : NaninovelTMProText, IRevealableText
    {
        private class TMProRevealBehaviour : TextRevealBehaviour
        {
            private readonly RevealableTMProText tmPro;

            public TMProRevealBehaviour (RevealableTMProText tmPro)
                : base(tmPro, tmPro.slideClipRect, tmPro.isRightToLeftText, tmPro.revealFadeWidth)
            {
                this.tmPro = tmPro;
            }

            protected override Vector2 GetTextRectSize () => new Vector2(tmPro.m_marginWidth, tmPro.m_marginHeight);
            protected override int GetCharacterCount () => tmPro.textInfo.characterCount;
            protected override RevealableCharacter GetCharacterAt (int index) => tmPro.GetCharacterAt(index);
            protected override RevealableLine GetLineAt (int index) => tmPro.GetLineAt(index);
            protected override IReadOnlyList<Material> GetMaterials () => tmPro.GetMaterials();
            protected override Vector4 GetClipRectScale () => new Vector4(tmPro.clipRectScale.x, tmPro.clipRectScale.y, tmPro.clipRectScale.x, tmPro.clipRectScale.y);
        }

        public virtual string Text { get => assignedText; set => SetTextToReveal(value); }
        public virtual Color TextColor { get => color; set => color = value; }
        public virtual GameObject GameObject => gameObject;
        public virtual bool Revealing => revealBehaviour.Revealing;
        public virtual float RevealProgress { get => revealBehaviour.GetRevealProgress(); set => revealBehaviour.SetRevealProgress(value); }

        [Tooltip("Width (in pixels) of the gradient fade near the reveal border.")]
        [SerializeField] private float revealFadeWidth = 100f;
        [Tooltip("Whether to smoothly reveal the text. Disable for the `typewriter` effect.")]
        [SerializeField] private bool slideClipRect = true;
        [Tooltip("How much to slant the reveal rect when passing over italic characters.")]
        [SerializeField] private float italicSlantAngle = 10f;
        [Tooltip("Scales the reveal clip rect by specified width (x) and height (y). By default, clip rect size is evaluated from text layout, but you may need to modify it in some cases (eg, when applying font dilate via shader).")]
        [SerializeField] private Vector2 clipRectScale = Vector2.one;

        private bool edited => !Application.isPlaying || ObjectUtils.IsEditedInPrefabMode(gameObject);
        private Material[] cachedFontMaterials;
        private TMProRevealBehaviour revealBehaviour;
        private string assignedText;

        public virtual void RevealNextChars (int count, float duration, AsyncToken asyncToken)
        {
            revealBehaviour.RevealNextChars(count, duration, asyncToken);
        }

        public virtual Vector2 GetLastRevealedCharPosition ()
        {
            return revealBehaviour.GetLastRevealedCharPosition();
        }

        public virtual char GetLastRevealedChar ()
        {
            if (string.IsNullOrEmpty(Text) || revealBehaviour.LastRevealedCharIndex < 0 || revealBehaviour.LastRevealedCharIndex >= Text.Length)
                return default;
            return Text[revealBehaviour.LastRevealedCharIndex];
        }

        public virtual void Render ()
        {
            if (edited) return;
            revealBehaviour.Render();
        }

        public override void Rebuild (CanvasUpdate update)
        {
            base.Rebuild(update);
            revealBehaviour?.Rebuild();
        }

        protected override void Awake ()
        {
            base.Awake();
            revealBehaviour = new TMProRevealBehaviour(this);
        }

        protected override void OnEnable ()
        {
            base.OnEnable();
            if (edited) return;
            RegisterDirtyLayoutCallback(revealBehaviour.WaitForRebuild);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            if (edited) return;
            UnregisterDirtyLayoutCallback(revealBehaviour.WaitForRebuild);
        }

        private void SetTextToReveal (string value)
        {
            assignedText = value;
            text = value;
        }

        private RevealableLine GetLineAt (int index)
        {
            if (index < 0 || index >= textInfo.lineInfo.Length)
                return RevealableLine.Invalid;

            var info = textInfo.lineInfo[index];
            return new RevealableLine(index, info.lineHeight, info.ascender, info.firstCharacterIndex, info.lastCharacterIndex);
        }

        private RevealableCharacter GetCharacterAt (int index)
        {
            if (index < 0 || index >= textInfo.characterInfo.Length)
                return RevealableCharacter.Invalid;

            var info = textInfo.characterInfo[index];
            var slantAngle = info.style == FontStyles.Italic ? italicSlantAngle : 0f;
            return new RevealableCharacter(index, info.lineNumber, info.origin, info.xAdvance, slantAngle, info.vertex_BR.position.x);
        }

        private Material[] GetMaterials ()
        {
            if (ShouldInvalidateMaterials())
                cachedFontMaterials = fontMaterials;
            return cachedFontMaterials;
        }

        private bool ShouldInvalidateMaterials ()
        {
            if (cachedFontMaterials is null || cachedFontMaterials.Length != textInfo.materialCount) return true;
            for (int i = 0; i < textInfo.materialCount; i++)
                if (cachedFontMaterials[i] != fontSharedMaterials[i])
                    return true;
            return false;
        }
    }
}
