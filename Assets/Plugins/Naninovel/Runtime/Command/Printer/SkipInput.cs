// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Can be used in generic text lines to prevent activating `wait for input` mode when the text is printed.
    /// </summary>
    public class SkipInput : Command
    {
        public override UniTask ExecuteAsync (AsyncToken asyncToken = default) => UniTask.CompletedTask;
    } 
}
