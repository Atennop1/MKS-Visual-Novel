// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="ITextPrinterActor"/>.
    /// </summary>
    [System.Serializable]
    public class TextPrinterState : ActorState<ITextPrinterActor>
    {
        /// <inheritdoc cref="ITextPrinterActor.Text"/>
        public string Text => text;
        /// <inheritdoc cref="ITextPrinterActor.AuthorId"/>
        public string AuthorId => authorId;
        /// <inheritdoc cref="ITextPrinterActor.RichTextTags"/>
        public List<string> RichTextTags => new List<string>(richTextTags);
        /// <inheritdoc cref="ITextPrinterActor.RevealProgress"/>
        public float RevealProgress => revealProgress;

        [SerializeField] private string text = default;
        [SerializeField] private string authorId = default;
        [SerializeField] private List<string> richTextTags = new List<string>();
        [SerializeField] private float revealProgress = 0f;

        public override void OverwriteFromActor (ITextPrinterActor actor)
        {
            base.OverwriteFromActor(actor);

            text = actor.Text;
            authorId = actor.AuthorId;
            richTextTags.Clear();
            if (actor.RichTextTags != null && actor.RichTextTags.Count > 0)
                richTextTags.AddRange(actor.RichTextTags);
            revealProgress = actor.RevealProgress;
        }

        public override void ApplyToActor (ITextPrinterActor actor)
        {
            base.ApplyToActor(actor);

            actor.Text = text;
            actor.AuthorId = authorId;
            actor.RichTextTags = new List<string>(richTextTags);
            actor.RevealProgress = revealProgress;
        }
    }
}
