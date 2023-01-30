// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Automatically save the game to a quick save slot.
    /// </summary>
    [CommandAlias("save")]
    public class AutoSave : Command
    {
        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            // Don't await here, otherwise script player won't be able to sync the running commands.
            Engine.GetService<IStateManager>().QuickSaveAsync().Forget();
            return UniTask.CompletedTask;
        }
    } 
}
