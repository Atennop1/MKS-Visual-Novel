// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="ITextPrinterActor"/>.
    /// </summary>
    [System.Serializable]
    public class TextPrinterMetadata : OrthoActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<TextPrinterMetadata> { }

        [Tooltip("Whether to automatically reset the printer on each `@print` command (unless `reset` parameter is explicitly disabled).")]
        public bool AutoReset = true;
        [Tooltip("Whether to automatically make the printer default and hide other printers on each `@print` command (unless `default` parameter is explicitly disabled).")]
        public bool AutoDefault = true;
        [Tooltip("Whether to automatically wait for user input on each `@print` command (unless `waitInput` parameter is explicitly disabled).")]
        public bool AutoWait = true;
        [Tooltip("Number of line breaks to automatically insert before the printed text on each `@print` command when the printer already contains some text (unless `br` parameter is explicitly specified).")]
        public int AutoLineBreak = 0;
        [Tooltip("Whether to add printed messages to the printer backlog.")]
        public bool AddToBacklog = true;
        [Tooltip("Whether to always split added backlog messages, even when the printer is not reset.")]
        public bool SplitBacklogMessages = false;
        [Tooltip("Whether to stop any playing voices on each `@print` command.")]
        public bool StopVoice = false;
        [Tooltip("Default visibility change animation duration; used when corresponding parameter is not specified in script command.")]
        public float ChangeVisibilityDuration = .3f;
        [Tooltip("Number of frames to wait before completing @print command. A value greater than zero is required to make the printed text visible while in skip mode.")]
        public int PrintFrameDelay = 1;
        [Tooltip("Template to apply for printed messages with an associated author. %TEXT% will be replaced with the message text and %AUTHOR% with the author name (character display name).")]
        public string AuthoredTemplate = default;

        public TextPrinterMetadata ()
        {
            Implementation = typeof(UITextPrinter).AssemblyQualifiedName;
            Loader = new ResourceLoaderConfiguration { PathPrefix = TextPrintersConfiguration.DefaultPathPrefix };
            Pivot = new Vector2(.5f, .5f);
        }

        public override ActorPose<TState> GetPoseOrNull<TState> (string poseName) => null;
    }
}
