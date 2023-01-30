// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel.Commands
{
    /// <summary>
    /// Navigates naninovel script playback to the provided path and saves that path to global state; 
    /// [@return] commands use this info to redirect to command after the last invoked gosub command. 
    /// </summary>
    /// <remarks>
    /// Designed to serve as a function (subroutine) in a programming language, allowing to reuse a piece of naninovel script.
    /// It's possible to declare a gosub outside of the currently played script and use it from any other scripts, which could be
    /// useful for invoking a repeating set of commands multiple times.
    /// </remarks>
    public class Gosub : Command, Command.IForceWait
    {
        /// <summary>
        /// Path to navigate into in the following format: `ScriptName.LabelName`.
        /// When label name is omitted, will play provided script from the start.
        /// When script name is omitted, will attempt to find a label in the currently played script.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ResourceContext(ScriptsConfiguration.DefaultPathPrefix, 0)]
        public NamedStringParameter Path;
        /// <summary>
        /// When specified, will reset the engine services state before loading a script (in case the path is leading to another script).
        /// Specify `*` to reset all the services, or specify service names to exclude from reset.
        /// By default, the state does not reset.
        /// </summary>
        [ParameterAlias("reset")]
        public StringListParameter ResetState;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var player = Engine.GetService<IScriptPlayer>();

            var spot = new PlaybackSpot(player.PlayedScript.Name, player.PlayedCommand?.PlaybackSpot.LineIndex + 1 ?? 0, 0);
            player.GosubReturnSpots.Push(spot);

            var resetState = Assigned(ResetState) ? ResetState : (StringListParameter)new List<string> { Goto.NoResetFlag };
            await new Goto { Path = Path, ResetState = resetState }.ExecuteAsync(asyncToken);
        }
    } 
}
