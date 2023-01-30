// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a text printer actor on scene.
    /// Text printers are able to gradually reveal text messages associated with <see cref="ICharacterActor"/>.
    /// </summary>
    public interface ITextPrinterActor : IActor
    {
        /// <summary>
        /// The text message which the printer has currently assigned.
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// ID of a character actor to which the currently assigned text belongs.
        /// </summary>
        string AuthorId { get; set; }
        /// <summary>
        /// Bodies of the rich text tags to apply for the assigned text,
        /// in order the tags were added.
        /// </summary>
        List<string> RichTextTags { get; set; }
        /// <summary>
        /// Which part of the assigned text message is currently revealed, in 0.0 to 1.0 range.
        /// </summary>
        float RevealProgress { get; set; }

        /// <summary>
        /// Reveals the assigned text message over time.
        /// </summary>
        /// <param name="revealDelay">Delay (in seconds) to wait after revealing each text character.</param>
        /// <param name="asyncToken">Token for task cancellation.</param>
        UniTask RevealTextAsync (float revealDelay, AsyncToken asyncToken = default);
    } 
}
