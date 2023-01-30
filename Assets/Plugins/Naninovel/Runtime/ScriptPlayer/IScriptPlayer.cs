// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using Naninovel.Commands;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to handle <see cref="Script"/> execution (playback).
    /// </summary>
    public interface IScriptPlayer : IEngineService<ScriptPlayerConfiguration>
    {
        /// <summary>
        /// Event invoked when player starts playing a script.
        /// </summary>
        event Action<Script> OnPlay;
        /// <summary>
        /// Event invoked when player stops playing a script.
        /// </summary>
        event Action<Script> OnStop;
        /// <summary>
        /// Event invoked when player starts executing a <see cref="Command"/>.
        /// </summary>
        event Action<Command> OnCommandExecutionStart;
        /// <summary>
        /// Event invoked when player finishes executing a <see cref="Command"/>.
        /// </summary>
        event Action<Command> OnCommandExecutionFinish;
        /// <summary>
        /// Event invoked when skip mode changes.
        /// </summary>
        event Action<bool> OnSkip;
        /// <summary>
        /// Event invoked when auto play mode changes.
        /// </summary>
        event Action<bool> OnAutoPlay;
        /// <summary>
        /// Event invoked when waiting for input mode changes.
        /// </summary>
        event Action<bool> OnWaitingForInput;
        /// <summary>
        /// Event invoked when script preload progress is changed, in 0.0 to 1.0 range.
        /// </summary>
        event Action<float> OnPreloadProgress;

        /// <summary>
        /// Whether script playback routine is currently running.
        /// </summary>
        bool Playing { get; }
        /// <summary>
        /// Whether skip mode is currently active.
        /// </summary>
        bool SkipActive { get; }
        /// <summary>
        /// Whether auto play mode is currently active.
        /// </summary>
        bool AutoPlayActive { get; }
        /// <summary>
        /// Whether user input is required to execute next script command.
        /// </summary>
        bool WaitingForInput { get; }
        /// <summary>
        /// Skip mode to use while <see cref="SkipActive"/>.
        /// </summary>
        PlayerSkipMode SkipMode { get; set; }
        /// <summary>
        /// Currently played <see cref="Script"/>.
        /// </summary>
        Script PlayedScript { get; }
        /// <summary>
        /// Currently played <see cref="Command"/>.
        /// </summary>
        Command PlayedCommand { get; }
        /// <summary>
        /// Currently played <see cref="Naninovel.PlaybackSpot"/>.
        /// </summary>
        PlaybackSpot PlaybackSpot { get; }
        /// <summary>
        /// List of <see cref="Command"/> built upon the currently played <see cref="Script"/>.
        /// </summary>
        ScriptPlaylist Playlist { get; }
        /// <summary>
        /// Index of the currently played command within the <see cref="Playlist"/>.
        /// </summary>
        int PlayedIndex { get; }
        /// <summary>
        /// Last playback return spots stack registered by <see cref="Gosub"/> commands.
        /// </summary>
        Stack<PlaybackSpot> GosubReturnSpots { get; }
        /// <summary>
        /// Total number of unique commands ever played by the player (global state scope).
        /// </summary>
        int PlayedCommandsCount { get; }
        
        /// <summary>
        /// Starts <see cref="PlayedScript"/> playback at <see cref="PlayedIndex"/>.
        /// </summary>
        void Play ();
        /// <summary>
        /// Starts <see cref="PlayedScript"/> playback using provided <paramref name="playlist"/>, starting at <paramref name="playlistIndex"/>.
        /// </summary>
        /// <param name="playlist">The playlist to use for playback.</param>
        /// <param name="playlistIndex">Playlist index to start playback from.</param>
        void Play (ScriptPlaylist playlist, int playlistIndex);
        /// <summary>
        /// Starts playback of the provided script at the provided line and inline indexes.
        /// </summary>
        /// <param name="script">The script to play.</param>
        /// <param name="startLineIndex">Line index to start playback from.</param>
        /// <param name="startInlineIndex">Command inline index to start playback from.</param>
        void Play (Script script, int startLineIndex = 0, int startInlineIndex = 0);
        /// <summary>
        /// Preloads the script's commands and starts playing at the provided line and inline indexes or a label;
        /// when <paramref name="label"/> is provided, will ignore line and inline indexes.
        /// </summary>
        /// <remarks>Preload progress is reported by <see cref="OnPreloadProgress"/> event.</remarks>
        /// <param name="script">The script to play.</param>
        /// <param name="startLineIndex">Line index to start playback from.</param>
        /// <param name="startInlineIndex">Command inline index to start playback from.</param>
        /// <param name="label">Name of a label within the script to start playback from.</param>
        UniTask PreloadAndPlayAsync (Script script, int startLineIndex = 0, int startInlineIndex = 0, string label = null);
        /// <summary>
        /// Halts the playback of the currently played script.
        /// </summary>
        void Stop ();
        /// <summary>
        /// Depending on whether the provided <paramref name="lineIndex"/> being before or after currently played command' line index,
        /// performs a fast-forward playback or state rollback of the currently loaded script.
        /// </summary>
        /// <param name="lineIndex">The line index to rewind at.</param>
        /// <returns>Whether the <paramref name="lineIndex"/> has been reached.</returns>
        UniTask<bool> RewindAsync (int lineIndex);
        /// <summary>
        /// Sets the player skip mode.
        /// </summary>
        void SetSkipEnabled (bool enabled);
        /// <summary>
        /// Sets the player auto play mode.
        /// </summary>
        void SetAutoPlayEnabled (bool enabled);
        /// <summary>
        /// Sets the player waiting for input mode.
        /// </summary>
        void SetWaitingForInputEnabled (bool enabled);
        /// <summary>
        /// Adds a task to perform before a command is executed.
        /// </summary>
        void AddPreExecutionTask (Func<Command, UniTask> task);
        /// <summary>
        /// Removes a task to perform before a command is executed.
        /// </summary>
        void RemovePreExecutionTask (Func<Command, UniTask> task);
        /// <summary>
        /// Adds a task to perform after a command is executed.
        /// </summary>
        void AddPostExecutionTask (Func<Command, UniTask> task);
        /// <summary>
        /// Removes a task to perform after a command is executed.
        /// </summary>
        void RemovePostExecutionTask (Func<Command, UniTask> task);
        /// <summary>
        /// Requests lazy cancellation of all the concurrently-running commands (via <see cref="AsyncToken.Completed"/>)
        /// and waits for them to finish before performing the provided task and executing next commands.
        /// </summary>
        UniTask SynchronizeAndDoAsync (Func<UniTask> task);
        /// <summary>
        /// Whether the player has ever played a command at the provided script and playlist index (global state).
        /// </summary>
        bool HasPlayed (string scriptName, int playlistIndex);
        /// <summary>
        /// Whether the player has ever played a script with the specified name.
        /// </summary>
        bool HasPlayed (string scriptName);
    }
}
