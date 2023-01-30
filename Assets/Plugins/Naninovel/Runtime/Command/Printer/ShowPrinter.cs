// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Shows a text printer.
    /// </summary>
    public class ShowPrinter : PrinterCommand
    {
        /// <summary>
        /// ID of the printer actor to use. Will use a default one when not provided.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ActorContext(TextPrintersConfiguration.DefaultPathPrefix)]
        public StringParameter PrinterId;
        /// <summary>
        /// Duration (in seconds) of the show animation.
        /// Default value for each printer is set in the actor configuration.
        /// </summary>
        [ParameterAlias("time")]
        public DecimalParameter Duration;

        protected override string AssignedPrinterId => PrinterId;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var printer = await GetOrAddPrinterAsync(asyncToken);
            var printerMeta = PrinterManager.Configuration.GetMetadataOrDefault(printer.Id);
            var showDuration = Assigned(Duration) ? Duration.Value : printerMeta.ChangeVisibilityDuration;
            if (asyncToken.Completed) printer.Visible = true;
            else await printer.ChangeVisibilityAsync(true, showDuration, asyncToken: asyncToken);
        }
    } 
}
