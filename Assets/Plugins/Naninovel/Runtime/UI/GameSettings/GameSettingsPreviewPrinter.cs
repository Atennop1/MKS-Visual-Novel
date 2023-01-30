// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Linq;
using System.Threading;
using Naninovel.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class GameSettingsPreviewPrinter : ScriptableUIBehaviour
    {
        protected virtual IRevealableText RevealableText { get; private set; }

        [Tooltip("A component implementing `" + nameof(IRevealableText) + "` interface for displaying the preview text.")]
        [SerializeField] private Graphic revealableText = default;

        private CancellationTokenSource revealCTS;
        private ITextPrinterManager printerManager;

        public override void Show ()
        {
            base.Show();

            StartPrinting();
        }

        public virtual void StartPrinting ()
        {
            revealCTS?.Cancel();

            RevealableText.RevealProgress = 0;
            if (RevealableText is Graphic graphic)
                graphic.Rebuild(CanvasUpdate.PreRender); // Otherwise it's not displaying anything.

            var revealDelay = Mathf.Lerp(printerManager.Configuration.MaxRevealDelay, 0, printerManager.BaseRevealSpeed);
            if (revealDelay == 0)
                RevealableText.RevealProgress = 1;
            else
            {
                revealCTS = new CancellationTokenSource();
                RevealTextOverTimeAsync(revealDelay, revealCTS.Token).Forget();
            }
        }

        protected override void Awake ()
        {
            base.Awake();

            RevealableText = revealableText as IRevealableText;
            if (RevealableText is null)
                throw new Exception($"Field `{nameof(revealableText)}` on `{nameof(GameSettingsPreviewPrinter)}` component is either not assigned or doesn't implement `{nameof(IRevealableText)}` interface.");
            printerManager = Engine.GetService<ITextPrinterManager>();
        }

        protected virtual void LateUpdate ()
        {
            if (Visible) RevealableText?.Render();
        }

        protected virtual async UniTask RevealTextOverTimeAsync (float revealDelay, CancellationToken cancellationToken)
        {
            var lastRevealTime = Time.time;
            while (RevealableText.RevealProgress < 1)
            {
                var timeSinceLastReveal = Time.time - lastRevealTime;
                var charsToReveal = Mathf.FloorToInt(timeSinceLastReveal / revealDelay);
                if (charsToReveal > 0)
                {
                    lastRevealTime = Time.time;
                    RevealableText.RevealNextChars(charsToReveal, revealDelay, cancellationToken);
                    while (RevealableText.Revealing && !cancellationToken.IsCancellationRequested)
                        await AsyncUtils.WaitEndOfFrameAsync();
                    if (cancellationToken.IsCancellationRequested) return;
                }
                await AsyncUtils.WaitEndOfFrameAsync();
            }

            var autoPlayDelay = Mathf.Lerp(0, printerManager.Configuration.MaxAutoWaitDelay, printerManager.BaseAutoDelay) * RevealableText.Text.Count(char.IsLetterOrDigit);
            var waitUntilTime = Time.time + autoPlayDelay;
            while (Time.time < waitUntilTime && !cancellationToken.IsCancellationRequested)
                await AsyncUtils.WaitEndOfFrameAsync();

            if (cancellationToken.IsCancellationRequested) return;

            StartPrinting();
        }
    }
}
