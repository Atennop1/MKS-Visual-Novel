// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Hides a text printer.
    /// </summary>
    public class HidePrinter : PrinterCommand
    {
        /// <summary>
        /// ID of the printer actor to use. Will use a default one when not provided.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ActorContext(TextPrintersConfiguration.DefaultPathPrefix)]
        public StringParameter PrinterId;
        /// <summary>
        /// Duration (in seconds) of the hide animation.
        /// Default value for each printer is set in the actor configuration.
        /// </summary>
        [ParameterAlias("time")]
        public DecimalParameter Duration;

        protected override string AssignedPrinterId => PrinterId;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var printer = await GetOrAddPrinterAsync(asyncToken);
            var printerMeta = PrinterManager.Configuration.GetMetadataOrDefault(printer.Id);
            var hideDuration = Assigned(Duration) ? Duration.Value : printerMeta.ChangeVisibilityDuration;
            if (asyncToken.Completed) printer.Visible = false;
            else await printer.ChangeVisibilityAsync(false, hideDuration, asyncToken: asyncToken);
        }
    }
}
