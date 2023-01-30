// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel
{
    /// <summary>
    /// Dictates when the resources are loaded and unloaded during script execution.
    /// </summary>
    public enum ResourcePolicy
    {
        /// <summary>
        /// All the resources required for the script execution are preloaded when starting 
        /// the playback and unloaded only when the script has finished playing.
        /// </summary>
        Static,
        /// <summary>
        /// Only the resources required for the next <see cref="ResourceProviderConfiguration.DynamicPolicySteps"/> commands
        /// are preloaded during the script execution and all the unused resources are unloaded immediately. 
        /// Use this mode when targeting platforms with strict memory limitations and it's impossible to properly organize naninovel scripts.
        /// </summary>
        Dynamic
    }
}
