// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    /// <summary>
    /// Represent available methods to associate voice clips with @print commands,
    /// when using <see cref="AudioConfiguration.EnableAutoVoicing"/>.
    /// </summary>
    public enum AutoVoiceMode
    {
        /// <summary>
        /// Voice clips are associated by <see cref="Command.PlaybackSpot"/> of the @print commands.
        /// </summary>
        PlaybackSpot,
        /// <summary>
        /// Voice clips are associated by <see cref="Commands.PrintText.AutoVoiceId"/>, 
        /// <see cref="Commands.PrintText.AuthorId"/> and <see cref="Commands.PrintText.Text"/>.
        /// </summary>
        ContentHash
    }
}
