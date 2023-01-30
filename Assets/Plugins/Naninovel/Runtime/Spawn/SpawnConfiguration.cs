// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class SpawnConfiguration : Configuration
    {
        public const string DefaultPathPrefix = "Spawn";
        /// <summary>
        /// Used to delimit spawned object ID from its path.
        /// </summary>
        public const string IdDelimiter = "#";

        [Tooltip("Configuration of the resource loader used with spawn resources.")]
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultPathPrefix };
        
        /// <summary>
        /// In case <paramref name="input"/> contains <see cref="IdDelimiter"/>, 
        /// extracts ID and returns path without the ID and delimiter; otherwise, returns input.
        /// </summary>
        public static string ProcessInputPath (string input, out string id)
        {
            if (input.Contains(IdDelimiter))
            {
                id = input.GetAfterFirst(IdDelimiter);
                return input.GetBefore(IdDelimiter);
            }

            id = null;
            return input;
        }
    }
}
