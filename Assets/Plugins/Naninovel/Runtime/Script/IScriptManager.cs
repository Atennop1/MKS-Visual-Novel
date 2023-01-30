// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage <see cref="Script"/> assets.
    /// </summary>
    public interface IScriptManager : IEngineService<ScriptsConfiguration>
    {
        /// <summary>
        /// Invoked when a script asset loading operation is started (multiple consequent operations are batched).
        /// </summary>
        event Action OnScriptLoadStarted;
        /// <summary>
        /// Invoked when a script asset loading operation is finished (multiple consequent operations are batched).
        /// </summary>
        event Action OnScriptLoadCompleted;

        /// <summary>
        /// Name of the script to play when starting a new game. 
        /// </summary>
        string StartGameScriptName { get; }
        /// <summary>
        /// UI used to navigate over the available script assets for debug purposes.
        /// </summary>
        UI.ScriptNavigatorPanel ScriptNavigator { get; }
        /// <summary>
        /// Total number of commands existing in all the available naninovel scripts.
        /// Only available when <see cref="ScriptsConfiguration.CountTotalCommands"/> is enabled.
        /// </summary>
        int TotalCommandsCount { get; }

        /// <summary>
        /// Locates all the available script assets.
        /// </summary>
        UniTask<IReadOnlyCollection<string>> LocateScriptsAsync ();
        /// <summary>
        /// Locates all the script assets stored under <see cref="ScriptsConfiguration.ExternalLoader"/> (community modding feature).
        /// </summary>
        UniTask<IReadOnlyCollection<string>> LocateExternalScriptsAsync ();
        /// <summary>
        /// Loads script asset with the provided name and related localization script (when available).
        /// </summary>
        UniTask<Script> LoadScriptAsync (string name);
        /// <summary>
        /// Loads all the available script assets and related localization scripts (when available).
        /// </summary>
        UniTask<IReadOnlyCollection<Script>> LoadAllScriptsAsync ();
        /// <summary>
        /// Unloads a previously loaded script asset with the provided name and related localization script (when available).
        /// </summary>
        void UnloadScript (string name);
        /// <summary>
        /// Unloads all previously loaded script assets and related localization scripts (when available).
        /// </summary>
        void UnloadAllScripts ();
        /// <summary>
        /// Attempts to retrieve a localization script for the provided script.
        /// Returns null if localization script doesn't exist, provided script not loaded 
        /// or game is running under <see cref="LocalizationConfiguration.SourceLocale"/>.
        /// </summary>
        /// <param name="script">The source script for which to request a localization counterpart. Should be loaded by the manager.</param>
        Script GetLocalizationScriptFor (Script script);
    }
}
