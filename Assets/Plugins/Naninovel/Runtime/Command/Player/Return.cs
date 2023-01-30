// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Attempts to navigate naninovel script playback to a command after the last used [@gosub].
    /// See [@gosub] command summary for more info and usage examples.
    /// </summary>
    public class Return : Command, Command.IForceWait
    {
        /// <summary>
        /// When specified, will reset the engine services state before returning to the initial script 
        /// from which the gosub was entered (in case it's not the currently played script).
        /// Specify `*` to reset all the services, or specify service names to exclude from reset.
        /// By default, the state does not reset.
        /// </summary>
        [ParameterAlias("reset")]
        public StringListParameter ResetState;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var player = Engine.GetService<IScriptPlayer>();

            if (player.GosubReturnSpots.Count == 0 || string.IsNullOrWhiteSpace(player.GosubReturnSpots.Peek().ScriptName))
            {
                LogWarningWithPosition("Failed to return to the last gosub: state data is missing or invalid.");
                return;
            }

            var spot = player.GosubReturnSpots.Pop();

            if (player.PlayedScript != null && player.PlayedScript.Name.EqualsFastIgnoreCase(spot.ScriptName))
            {
                player.Play(player.PlayedScript, spot.LineIndex);
                return;
            }

            var stateManager = Engine.GetService<IStateManager>();

            // Reset `all` is requested.
            if (Assigned(ResetState) && ResetState.Length == 1 && ResetState[0] == "*")
            {
                await stateManager.ResetStateAsync(
                    () => player.PreloadAndPlayAsync(spot.ScriptName, spot.LineIndex));
                return;
            }

            // Requested reset excluding specific services.
            if (Assigned(ResetState) && ResetState.Length > 0)
            {
                await stateManager.ResetStateAsync(ResetState, () => player.PreloadAndPlayAsync(spot.ScriptName, spot.LineIndex));
                return;
            }

            // No reset is needed, just loading the script.
            await player.PreloadAndPlayAsync(spot.ScriptName, spot.LineIndex);
        }
    } 
}
