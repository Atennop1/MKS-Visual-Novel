// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    public abstract class PrinterCommand : Command
    {
        protected abstract string AssignedPrinterId { get; }
        protected virtual string AssignedAuthorId => null;

        protected ITextPrinterManager PrinterManager => Engine.GetService<ITextPrinterManager>();
        protected ICharacterManager CharacterManager => Engine.GetService<ICharacterManager>();
        protected TextPrintersConfiguration Configuration => PrinterManager.Configuration;

        private ITextPrinterActor heldPrinterActor;

        public virtual async UniTask PreloadResourcesAsync ()
        {
            heldPrinterActor = await GetOrAddPrinterAsync();
            await heldPrinterActor.HoldResourcesAsync(null, this);
        }

        public virtual void ReleasePreloadedResources ()
        {
            heldPrinterActor?.ReleaseResources(null, this);
        }

        protected virtual async UniTask<ITextPrinterActor> GetOrAddPrinterAsync (AsyncToken asyncToken = default)
        {
            var printerId = default(string);

            if (string.IsNullOrEmpty(AssignedPrinterId) && !string.IsNullOrEmpty(AssignedAuthorId))
                printerId = CharacterManager.Configuration.GetMetadataOrDefault(AssignedAuthorId).LinkedPrinter;
            
            if (string.IsNullOrEmpty(printerId))
                printerId = AssignedPrinterId;

            var printer = await PrinterManager.GetOrAddActorAsync(printerId ?? PrinterManager.DefaultPrinterId);
            asyncToken.ThrowIfCanceled();
            return printer;
        }
    }
}
