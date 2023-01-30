// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class TextPrintersConfiguration : OrthoActorManagerConfiguration<TextPrinterMetadata>
    {
        public const string DefaultPathPrefix = "TextPrinters";

        [Tooltip("ID of the text printer to use by default.")]
        public string DefaultPrinterId = "Dialogue";
        [Range(0f, 1f), Tooltip("Base reveal speed (game settings) to set when the game is first started.")]
        public float DefaultBaseRevealSpeed = .5f;
        [Range(0f, 1f), Tooltip("Base auto delay (game settings) to set when the game is first started.")]
        public float DefaultBaseAutoDelay = .5f;
        [Tooltip("Delay limit (in seconds) when revealing (printing) the text messages. Specific reveal speed is set via `message speed` in the game settings; this value defines the available range (higher the value, lower the reveal speed)."), Range(.01f, 1f)]
        public float MaxRevealDelay = .06f;
        [Tooltip("Delay limit (in seconds) per each printed character while waiting to continue in auto play mode. Specific delay is set via `auto delay` in the game settings; this value defines the available range."), Range(.0f, .5f)]
        public float MaxAutoWaitDelay = .02f;
        [Tooltip("Whether to scale the wait time in auto play mode by the reveal speed set in the print commands.")]
        public bool ScaleAutoWait = true;
        [Tooltip("Metadata to use by default when creating text printer actors and custom metadata for the created actor ID doesn't exist.")]
        public TextPrinterMetadata DefaultMetadata = new TextPrinterMetadata();
        [Tooltip("Metadata to use when creating text printer actors with specific IDs.")]
        public TextPrinterMetadata.Map Metadata = new TextPrinterMetadata.Map {
            ["Dialogue"] = CreateBuiltinMeta(),
            ["Fullscreen"] = CreateBuiltinMeta(false, 2, authoredTemplate: "%AUTHOR%: “%TEXT%” "),
            ["Wide"] = CreateBuiltinMeta(),
            ["Chat"] = CreateBuiltinMeta(false, 0, true),
            ["Bubble"] = CreateBuiltinMeta(),
            ["TMProDialogue"] = CreateBuiltinMeta(),
            ["TMProFullscreen"] = CreateBuiltinMeta(false, 2, authoredTemplate: "%AUTHOR%: “%TEXT%” "),
            ["TMProWide"] = CreateBuiltinMeta(),
            ["TMProChat"] = CreateBuiltinMeta(false, 0, true),
            ["TMProBubble"] = CreateBuiltinMeta()
        };

        public override TextPrinterMetadata DefaultActorMetadata => DefaultMetadata;
        public override ActorMetadataMap<TextPrinterMetadata> ActorMetadataMap => Metadata;

        public TextPrintersConfiguration ()
        {
            AutoShowOnModify = false;
            ZOffset = 0;
            ZStep = 0;
        }

        private static TextPrinterMetadata CreateBuiltinMeta (bool autoReset = true, int autoBr = 0, 
            bool splitBacklog = false, string authoredTemplate = null) => new TextPrinterMetadata {
            Implementation = typeof(UITextPrinter).AssemblyQualifiedName,
            AutoReset = autoReset,
            AutoLineBreak = autoBr,
            SplitBacklogMessages = splitBacklog,
            AuthoredTemplate = authoredTemplate
        };
    }
}
