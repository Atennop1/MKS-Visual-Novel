// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEditor;

namespace Naninovel
{
    public class TextPrintersSettings : OrthoActorManagerSettings<TextPrintersConfiguration, ITextPrinterActor, TextPrinterMetadata>
    {
        protected override string HelpUri => "guide/text-printers.html";
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override MetadataEditor<ITextPrinterActor, TextPrinterMetadata> MetadataEditor { get; } = new TextPrinterMetadataEditor();

        private string GetTooltip ()
        {
            if (EditedActorId == Configuration.DefaultPrinterId)
                return "This printer will be active by default: all the generic text and `@print` commands will use it to output the text. Use `@printer PrinterID` action to change active printer.";
            return $"Use `@printer {EditedActorId}` in naninovel scripts to set this printer active; all the consequent generic text and `@print` commands will then use it to output the text.";
        }

        [MenuItem("Naninovel/Resources/Text Printers")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
