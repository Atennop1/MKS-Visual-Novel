// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Used by <see cref="UITextPrinter"/> to control the printed text.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UITextPrinterPanel : CustomUI, IManagedUI
    {
        /// <summary>
        /// Contents of the printer to be used for transformations.
        /// </summary>
        public virtual RectTransform Content => content;
        /// <summary>
        /// The text to be printed inside the printer panel. 
        /// Note that the visibility of the text is controlled independently.
        /// </summary>
        public abstract string PrintedText { get; set; }
        /// <summary>
        /// Text representing name of the author of the currently printed text.
        /// </summary>
        public abstract string AuthorNameText { get; set; }
        /// <summary>
        /// Which part of the assigned text message is currently revealed, in 0.0 to 1.0 range.
        /// </summary>
        public abstract float RevealProgress { get; set; }
        /// <summary>
        /// Current appearance of the printer.
        /// </summary>
        public abstract string Appearance { get; set; }
        /// <summary>
        /// Current tint color of the printer.
        /// </summary>
        public virtual Color TintColor { get => tintColor; set { tintColor = value; onTintChanged?.Invoke(value); } }
        /// <summary>
        /// Objects that should trigger continue input when interacted with.
        /// </summary>
        public virtual IReadOnlyCollection<GameObject> ContinueInputTriggers => continueInputTriggers;

        protected ICharacterManager CharacterManager { get; private set; }

        [Tooltip("Transform used for printer position, scale and rotation external manipulations.")]
        [SerializeField] private RectTransform content = default;
        [Tooltip("Objects that should trigger continue input when interacted with. Make sure the objects are a raycast target and not blocked by other raycast targets.")]
        [SerializeField] private List<GameObject> continueInputTriggers = default;
        [Tooltip("Event invoked when tint color of the printer actor is changed.")]
        [SerializeField] private ColorUnityEvent onTintChanged = default;

        private IInputSampler continueInput;
        private IScriptPlayer scriptPlayer;
        private Color tintColor = Color.white;

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            if (continueInput != null)
                foreach (var go in ContinueInputTriggers)
                    continueInput.AddObjectTrigger(go);
            scriptPlayer.OnWaitingForInput += SetWaitForInputIndicatorVisible;
        }

        UniTask IManagedUI.ChangeVisibilityAsync (bool visible, float? duration, AsyncToken asyncToken)
        {
            Debug.LogError("@showUI and @hideUI commands can't be used with text printers; use @show/hide or @show/hidePrinter commands instead");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Reveals the <see cref="PrintedText"/> char by char over time.
        /// </summary>
        /// <param name="revealDelay">Delay (in seconds) between revealing consequent characters.</param>
        /// <param name="asyncToken">The reveal should be canceled when requested by the provided token.</param>
        public abstract UniTask RevealPrintedTextOverTimeAsync (float revealDelay, AsyncToken asyncToken);
        /// <summary>
        /// Controls visibility of the wait for input indicator.
        /// </summary>
        public abstract void SetWaitForInputIndicatorVisible (bool visible);
        /// <summary>
        /// Invoked by <see cref="UITextPrinter"/> when author meta of the printed text changes.
        /// </summary>
        /// <param name="authorId">Actor ID of the new author.</param>
        /// <param name="authorMeta">Metadata of the new author.</param>
        public abstract void OnAuthorChanged (string authorId, CharacterMetadata authorMeta);

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(content);

            continueInput = Engine.GetService<IInputManager>().GetContinue();
            scriptPlayer = Engine.GetService<IScriptPlayer>();

            CharacterManager = Engine.GetService<ICharacterManager>();
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            if (continueInput != null)
                foreach (var go in ContinueInputTriggers)
                    continueInput.RemoveObjectTrigger(go);
            if (scriptPlayer != null)
                scriptPlayer.OnWaitingForInput -= SetWaitForInputIndicatorVisible;
        }
    } 
}
