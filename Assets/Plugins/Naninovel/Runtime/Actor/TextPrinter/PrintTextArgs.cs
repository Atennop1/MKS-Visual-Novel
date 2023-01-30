// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the print text events invoked by <see cref="ITextPrinterManager"/>. 
    /// </summary>
    public class PrintTextArgs : EventArgs
    {
        /// <summary>
        /// Printer actor which is printing the text.
        /// </summary>
        public readonly ITextPrinterActor Printer;
        /// <summary>
        /// Text of the message to print.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// ID of a character actor to which the printed text belongs (if any).
        /// </summary>
        public readonly string AuthorId;
        /// <summary>
        /// Text print speed (base reveal speed modifier).
        /// </summary>
        public readonly float Speed;

        public PrintTextArgs (ITextPrinterActor printer, string text, string authorId, float speed)
        {
            Printer = printer;
            Text = text;
            AuthorId = authorId;
            Speed = speed;
        }
    }
}