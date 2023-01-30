// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Appends provided text to a text printer.
    /// </summary>
    /// <remarks>
    /// The entire text will be appended immediately, without triggering reveal effect or any other side-effects.
    /// </remarks>
    [CommandAlias("append")]
    public class AppendText : PrinterCommand, Command.ILocalizable
    {
        /// <summary>
        /// The text to append.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, LocalizableParameter]
        public StringParameter Text;
        /// <summary>
        /// ID of the printer actor to use. Will use a a default one when not provided.
        /// </summary>
        [ParameterAlias("printer"), ActorContext(TextPrintersConfiguration.DefaultPathPrefix)]
        public StringParameter PrinterId;
        /// <summary>
        /// ID of the actor, which should be associated with the appended text.
        /// </summary>
        [ParameterAlias("author"), ActorContext(CharactersConfiguration.DefaultPathPrefix)]
        public StringParameter AuthorId;

        protected override string AssignedPrinterId => PrinterId;
        protected override string AssignedAuthorId => AuthorId;
        protected IUIManager UIManager => Engine.GetService<IUIManager>();

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var printer = await GetOrAddPrinterAsync(asyncToken);
            printer.Text += Text;
            printer.RevealProgress = 1f;
            UIManager.GetUI<UI.IBacklogUI>()?.AppendMessage(Text);
        }
    } 
}
