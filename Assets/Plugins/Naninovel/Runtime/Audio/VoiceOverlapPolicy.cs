// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel
{
    /// <summary>
    /// Dictates how to handle concurrent voices playback.
    /// </summary>
    public enum VoiceOverlapPolicy
    {
        /// <summary>
        /// Concurrent voices will be played without limitation.
        /// </summary>
        AllowOverlap,
        /// <summary>
        /// Prevent concurrent voices playback by stopping any played voice clip before playing a new one.
        /// </summary>
        PreventOverlap,
        /// <summary>
        /// Prevent concurrent voices playback per character; voices of different characters (auto voicing) 
        /// and any number of `@voice` command are allowed to be played concurrently.
        /// </summary>
        PreventCharacterOverlap
    }
}