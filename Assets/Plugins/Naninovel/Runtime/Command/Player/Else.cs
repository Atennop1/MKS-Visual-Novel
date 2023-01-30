// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Marks a branch of a conditional execution block,
    /// which is always executed in case conditions of the opening [@if] and all the preceding [@elseif] (if any) commands are not met.
    /// For usage examples see [conditional execution](/guide/naninovel-scripts.md#conditional-execution) guide.
    /// </summary>
    public class Else : Command
    {
        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            // We might get here either on exiting from an @if or @elseif branch (which condition is met), or via direct @goto playback jump. 
            // In any case, we just need to get out of the current conditional block.
            BeginIf.HandleConditionalBlock(true);

            return UniTask.CompletedTask;
        }
    }
}
