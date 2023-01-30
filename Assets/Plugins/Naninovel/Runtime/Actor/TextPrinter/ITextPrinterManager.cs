// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage <see cref="ITextPrinterActor"/> actors.
    /// </summary>
    public interface ITextPrinterManager : IActorManager<ITextPrinterActor, TextPrinterState, TextPrinterMetadata, TextPrintersConfiguration>
    {
        /// <summary>
        /// Invoked when a print text operation is started.
        /// </summary>
        event Action<PrintTextArgs> OnPrintTextStarted;
        /// <summary>
        /// Invoked when a print text operation is finished.
        /// </summary>
        event Action<PrintTextArgs> OnPrintTextFinished;

        /// <summary>
        /// ID of the printer actor to use by default when a specific one is not specified.
        /// </summary>
        string DefaultPrinterId { get; set; }
        /// <summary>
        /// Base speed for revealing text messages as per the game settings, in 0.0 to 1.0 range.
        /// </summary>
        float BaseRevealSpeed { get; set; }
        /// <summary>
        /// Base delay while waiting to continue in auto play mode (scaled by printed characters count) as per the game settings, in 0.0 to 1.0 range.
        /// </summary>
        float BaseAutoDelay { get; set; }

        /// <summary>
        /// Prints (reveals) provided text message over time using a managed text printer with the provided ID.
        /// </summary>
        /// <param name="printerId">ID of the managed text printer which should print the message.</param>
        /// <param name="text">Text of the message to print.</param>
        /// <param name="authorId">ID of a character actor to which the printed text belongs (if any).</param>
        /// <param name="speed">Text reveal speed (<see cref="BaseRevealSpeed"/> modifier).</param>
        /// <param name="asyncToken">Token for task cancellation. The text will be revealed instantly when cancelled.</param>
        UniTask PrintTextAsync (string printerId, string text, string authorId = default, float speed = 1, AsyncToken asyncToken = default);
    }
}
