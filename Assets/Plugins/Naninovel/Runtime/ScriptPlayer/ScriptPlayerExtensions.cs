// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Provides extension methods for <see cref="IScriptPlayer"/>.
    /// </summary>
    public static class ScriptPlayerExtensions
    {
        /// <summary>
        /// Loads the provided script, preloads the script's commands and starts playing at the provided line and inline indexes or a label;
        /// when <paramref name="label"/> is provided, will ignore line and inline indexes.
        /// </summary>
        /// <remarks>Preload progress is reported by <see cref="IScriptPlayer.OnPreloadProgress"/> event.</remarks>
        /// <param name="scriptName">Name (resource path) of the script to load and play.</param>
        /// <param name="startLineIndex">Line index to start playback from.</param>
        /// <param name="startInlineIndex">Command inline index to start playback from.</param>
        /// <param name="label">Name of a label within the script to start playback from.</param>
        public static async UniTask PreloadAndPlayAsync (this IScriptPlayer scriptPlayer, string scriptName, int startLineIndex = 0, int startInlineIndex = 0, string label = null)
        {
            var script = await Engine.GetService<IScriptManager>().LoadScriptAsync(scriptName);
            if (script is null) throw new Exception($"Script player failed to start: script with name `{scriptName}` wasn't able to load.");
            await scriptPlayer.PreloadAndPlayAsync(script, startLineIndex, startInlineIndex, label);
        }
    }
}
