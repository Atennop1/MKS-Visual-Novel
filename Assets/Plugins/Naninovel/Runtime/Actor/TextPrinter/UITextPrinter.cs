// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using Naninovel.UI;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ITextPrinterActor"/> implementation using <see cref="UITextPrinterPanel"/> to represent the actor.
    /// </summary>
    [ActorResources(typeof(UITextPrinterPanel), false)]
    public class UITextPrinter : MonoBehaviourActor<TextPrinterMetadata>, ITextPrinterActor
    {
        public override string Appearance { get => PrinterPanel.Appearance; set => PrinterPanel.Appearance = value; }
        public override bool Visible { get => PrinterPanel.Visible; set => PrinterPanel.Visible = value; }
        public virtual string Text { get => text; set => SetText(value); }
        public virtual string AuthorId { get => authorId; set => SetAuthorId(value); }
        public virtual List<string> RichTextTags { get => richTextTags; set => SetRichTextTags(value); }
        public virtual float RevealProgress { get => PrinterPanel.RevealProgress; set { CancelRevealTextRoutine(); PrinterPanel.RevealProgress = value; } }

        protected virtual UITextPrinterPanel PrinterPanel { get; private set; }
        protected virtual bool UsingRichTags => richTextTags.Count > 0;

        private readonly List<string> richTextTags = new List<string>();
        private readonly IUIManager uiManager;
        private readonly ICharacterManager characterManager;
        private readonly AspectMonitor aspectMonitor;
        private string text, authorId;
        private CancellationTokenSource revealTextCTS;
        private string activeOpenTags, activeCloseTags;

        public UITextPrinter (string id, TextPrinterMetadata metadata)
            : base(id, metadata)
        {
            uiManager = Engine.GetService<IUIManager>();
            characterManager = Engine.GetService<ICharacterManager>();
            activeOpenTags = string.Empty;
            activeCloseTags = string.Empty;
            aspectMonitor = new AspectMonitor();
        }

        public override async UniTask InitializeAsync ()
        {
            await base.InitializeAsync();

            var providerManager = Engine.GetService<IResourceProviderManager>();
            var localizationManager = Engine.GetService<ILocalizationManager>();
            var prefabResource = await ActorMetadata.Loader.CreateLocalizableFor<GameObject>(providerManager, localizationManager).LoadAsync(Id);
            if (!prefabResource.Valid) throw new Exception($"Failed to load `{Id}` UI text printer resource object. Make sure the printer is correctly configured.");

            PrinterPanel = await uiManager.AddUIAsync(prefabResource.Object) as UITextPrinterPanel;
            if (PrinterPanel == null) throw new Exception($"Failed to initialize `{Id}` printer actor: printer panel UI instantiation failed.");
            PrinterPanel.transform.SetParent(Transform);
            PrinterPanel.PrintedText = string.Empty;
            RevealProgress = 0f;

            aspectMonitor.OnChanged += HandleAspectChanged;
            aspectMonitor.Start(target: PrinterPanel);

            SetAuthorId(null);
            Visible = false;
        }

        public override UniTask ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default,
            Transition? transition = default, AsyncToken asyncToken = default)
        {
            Appearance = appearance;
            return UniTask.CompletedTask;
        }

        public override async UniTask ChangeVisibilityAsync (bool visible, float duration, EasingType easingType = default, AsyncToken asyncToken = default)
        {
            await PrinterPanel.ChangeVisibilityAsync(visible, duration, asyncToken);
        }

        public virtual async UniTask RevealTextAsync (float revealDelay, AsyncToken asyncToken = default)
        {
            CancelRevealTextRoutine();

            revealTextCTS = CancellationTokenSource.CreateLinkedTokenSource(asyncToken.CompletionToken);
            var revealTextToken = new AsyncToken(asyncToken.CancellationToken, revealTextCTS.Token);
            await PrinterPanel.RevealPrintedTextOverTimeAsync(revealDelay, revealTextToken);
        }

        public virtual void SetRichTextTags (List<string> tags)
        {
            richTextTags.Clear();

            if (tags?.Count > 0)
                richTextTags.AddRange(tags);

            if (UsingRichTags)
            {
                activeOpenTags = GetActiveTagsOpenSequence();
                activeCloseTags = GetActiveTagsCloseSequence();
            }
            else
            {
                activeOpenTags = string.Empty;
                activeCloseTags = string.Empty;
            }

            SetText(Text); // Update the printed text with the tags.
        }

        public virtual void SetAuthorId (string authorId)
        {
            this.authorId = authorId;

            // Attempt to find a character display name for the provided actor ID.
            var displayName = characterManager.GetDisplayName(authorId) ?? authorId;
            PrinterPanel.AuthorNameText = displayName;

            // Update author meta.
            var authorMeta = characterManager.Configuration.GetMetadataOrDefault(authorId);
            PrinterPanel.OnAuthorChanged(authorId, authorMeta);
        }

        public override void Dispose ()
        {
            base.Dispose();

            aspectMonitor?.Stop();

            CancelRevealTextRoutine();

            if (PrinterPanel != null)
            {
                uiManager.RemoveUI(PrinterPanel);
                ObjectUtils.DestroyOrImmediate(PrinterPanel.gameObject);
                PrinterPanel = null;
            }
        }

        protected override Vector3 GetBehaviourPosition ()
        {
            // Changing transform of the root obj won't work; modify content panel instead.
            if (!PrinterPanel || !PrinterPanel.Content) return Vector3.zero;

            // Printer position is always relative (0.0-1.0) to parent rect.
            var scenePos = new Vector3(
                PrinterPanel.Content.anchoredPosition.x / PrinterPanel.RectTransform.rect.width,
                PrinterPanel.Content.anchoredPosition.y / PrinterPanel.RectTransform.rect.height,
                PrinterPanel.Content.localPosition.z);

            return scenePos;
        }

        protected override void SetBehaviourPosition (Vector3 position)
        {
            // Changing transform of the root obj won't work; modify content panel instead.
            if (!PrinterPanel || !PrinterPanel.Content) return;

            // Printer position is always relative (0.0-1.0) to parent rect.
            var anchoredPos = new Vector2(
                position.x * PrinterPanel.RectTransform.rect.width,
                position.y * PrinterPanel.RectTransform.rect.height);

            PrinterPanel.Content.anchoredPosition = anchoredPos;
            PrinterPanel.Content.SetPosZ(position.z, true);
        }

        protected override Quaternion GetBehaviourRotation ()
        {
            if (!PrinterPanel || !PrinterPanel.Content) return Quaternion.identity;
            return PrinterPanel.Content.rotation;
        }

        protected override void SetBehaviourRotation (Quaternion rotation)
        {
            if (!PrinterPanel || !PrinterPanel.Content) return;
            PrinterPanel.Content.rotation = rotation;
        }

        protected override Vector3 GetBehaviourScale ()
        {
            if (!PrinterPanel || !PrinterPanel.Content) return Vector3.one;
            return PrinterPanel.Content.localScale;
        }

        protected override void SetBehaviourScale (Vector3 scale)
        {
            if (!PrinterPanel || !PrinterPanel.Content) return;
            PrinterPanel.Content.localScale = scale;
        }

        protected override Color GetBehaviourTintColor () => PrinterPanel.TintColor;

        protected override void SetBehaviourTintColor (Color value) => PrinterPanel.TintColor = value;

        protected virtual void SetText (string value)
        {
            if (value is null)
                value = string.Empty;

            text = value;

            // Handle rich text tags before assigning the actual text.
            PrinterPanel.PrintedText = UsingRichTags ? string.Concat(activeOpenTags, value, activeCloseTags) : value;
        }

        protected virtual void HandleAspectChanged (AspectMonitor monitor)
        {
            // UI printers anchored to canvas borders are moved on aspect change;
            // re-set position here to return them to correct relative positions.
            SetBehaviourPosition(Position);
        }

        private void CancelRevealTextRoutine ()
        {
            revealTextCTS?.Cancel();
            revealTextCTS?.Dispose();
            revealTextCTS = null;
        }

        private string GetActiveTagsOpenSequence ()
        {
            var result = string.Empty;

            if (RichTextTags is null || RichTextTags.Count == 0)
                return result;

            foreach (var tag in RichTextTags)
                result += $"<{tag}>";

            return result;
        }

        private string GetActiveTagsCloseSequence ()
        {
            var result = string.Empty;

            if (RichTextTags is null || RichTextTags.Count == 0)
                return result;

            var reversedActiveTags = RichTextTags;
            reversedActiveTags.Reverse();
            foreach (var tag in reversedActiveTags)
                result += $"</{tag.GetBefore("=") ?? tag}>";

            return result;
        }
    }
}
