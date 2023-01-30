// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Resets (clears) the contents of a text printer.
    /// </summary>
    public class ResetText : PrinterCommand
    {
        /// <summary>
        /// ID of the printer actor to use. Will use a default one when not provided.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ActorContext(TextPrintersConfiguration.DefaultPathPrefix)]
        public StringParameter PrinterId;

        protected override string AssignedPrinterId => PrinterId;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var printer = await GetOrAddPrinterAsync(asyncToken);
            printer.Text = string.Empty;
            printer.RevealProgress = 0f;
        }
    } 
}
