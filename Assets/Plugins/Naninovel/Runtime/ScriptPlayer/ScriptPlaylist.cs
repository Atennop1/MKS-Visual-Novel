// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a list of <see cref="Command"/> based on the contents of a <see cref="Script"/>.
    /// </summary>
    public class ScriptPlaylist : List<Command>
    {
        /// <summary>
        /// Name of the script from which the contained commands were extracted.
        /// </summary>
        public readonly string ScriptName;

        /// <summary>
        /// Creates new instance from the provided commands collection.
        /// </summary>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public ScriptPlaylist (string scriptName, IEnumerable<Command> commands)
            : base(commands)
        {
            ScriptName = scriptName;
        }

        /// <summary>
        /// Creates new instance from the provided script.
        /// When <paramref name="scriptManager"/> provided, will attempt to use a corresponding localization script.
        /// </summary>
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        public ScriptPlaylist (Script script, IScriptManager scriptManager = null)
        {
            ScriptName = script.Name;
            var localizationScript = scriptManager?.GetLocalizationScriptFor(script);
            var commands = script.ExtractCommands(localizationScript);
            AddRange(commands);
        }

        /// <summary>
        /// Preloads and holds all the resources required to execute <see cref="Command.IPreloadable"/> commands contained in this list.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public async UniTask PreloadResourcesAsync () => await PreloadResourcesAsync(0, Count - 1);

        /// <summary>
        /// Preloads and holds resources required to execute <see cref="Command.IPreloadable"/> commands in the specified range.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public async UniTask PreloadResourcesAsync (int startCommandIndex, int endCommandIndex, Action<float> onProgress = default)
        {
            if (Count == 0) return;

            if (!this.IsIndexValid(startCommandIndex) || !this.IsIndexValid(endCommandIndex) || endCommandIndex < startCommandIndex)
                throw new Exception($"Failed to preload `{ScriptName}` script resources: [{startCommandIndex}, {endCommandIndex}] is not a valid range.");

            onProgress?.Invoke(0);
            var count = endCommandIndex + 1 - startCommandIndex;
            var commandsToHold = GetRange(startCommandIndex, count).OfType<Command.IPreloadable>().ToArray();
            var heldCommands = 0;
            await UniTask.WhenAll(commandsToHold.Select(PreloadCommand));

            async UniTask PreloadCommand (Command.IPreloadable command)
            {
                await command.PreloadResourcesAsync();
                onProgress?.Invoke(++heldCommands / (float)commandsToHold.Length);
            }
        }

        /// <summary>
        /// Releases all the held resources required to execute <see cref="Command.IPreloadable"/> commands contained in this list.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public void ReleaseResources () => ReleaseResources(0, Count - 1);

        /// <summary>
        /// Releases all the held resources required to execute <see cref="Command.IPreloadable"/> commands in the specified range.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public void ReleaseResources (int startCommandIndex, int endCommandIndex)
        {
            if (Count == 0) return;

            if (!this.IsIndexValid(startCommandIndex) || !this.IsIndexValid(endCommandIndex) || endCommandIndex < startCommandIndex)
                throw new Exception($"Failed to unload `{ScriptName}` script resources: [{startCommandIndex}, {endCommandIndex}] is not a valid range.");

            var commandsToRelease = GetRange(startCommandIndex, (endCommandIndex + 1) - startCommandIndex).OfType<Command.IPreloadable>();
            foreach (var cmd in commandsToRelease)
                cmd.ReleasePreloadedResources();
        }

        /// <summary>
        /// Returns a <see cref="Command"/> at the provided index; null if not found.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetCommandByIndex (int commandIndex) => this.IsIndexValid(commandIndex) ? this[commandIndex] : null;

        /// <summary>
        /// Finds a <see cref="Command"/> that was created from a <see cref="CommandScriptLine"/> with provided line and inline indexes; null if not found.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetCommandByLine (int lineIndex, int inlineIndex) => Find(a => a.PlaybackSpot.LineIndex == lineIndex && a.PlaybackSpot.InlineIndex == inlineIndex);

        /// <summary>
        /// Finds a <see cref="Command"/> that was created from a <see cref="CommandScriptLine"/> located at or after the provided line and inline indexes; null if not found.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetCommandAfterLine (int lineIndex, int inlineIndex) => this.FirstOrDefault(a => a.PlaybackSpot.LineIndex >= lineIndex && a.PlaybackSpot.InlineIndex >= inlineIndex);

        /// <summary>
        /// Finds a <see cref="Command"/> that was created from a <see cref="CommandScriptLine"/> located at or before the provided line and inline indexes; null if not found.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetCommandBeforeLine (int lineIndex, int inlineIndex) => this.LastOrDefault(a => a.PlaybackSpot.LineIndex <= lineIndex && a.PlaybackSpot.InlineIndex <= inlineIndex);

        /// <summary>
        /// Returns first command in the list or null when the list is empty.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetFirstCommand () => this.FirstOrDefault();

        /// <summary>
        /// Returns last command in the list or null when the list is empty.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public Command GetLastCommand () => this.LastOrDefault();

        /// <summary>
        /// Finds index of a contained command with the provided playback spot or -1 when not found.
        /// </summary>
        [CollectionAccess(CollectionAccessType.Read)]
        public int IndexOf (PlaybackSpot playbackSpot)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].PlaybackSpot == playbackSpot)
                    return i;
            return -1;
        }

        /// <summary>
        /// Executes commands in the playlist independently of the current script player state.
        /// </summary>
        /// <remarks>
        /// Can be used to additively play a list of commands (not a real script), without interrupting currently played script.
        /// </remarks>
        public async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            foreach (var command in this)
            {
                if (!command.ShouldExecute) continue;
                if (Engine.GetConfiguration<ScriptPlayerConfiguration>().ShouldWait(command))
                    await command.ExecuteAsync(asyncToken);
                else command.ExecuteAsync(asyncToken).Forget();
                asyncToken.ThrowIfCanceled();
            }
        }
    }
}
