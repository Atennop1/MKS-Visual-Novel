// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Opens specified URL (web address) with a default web browser.
    /// </summary>
    /// <remarks>
    /// Unity's `Application.OpenURL` method is used to handle the command;
    /// consult the [documentation](https://docs.unity3d.com/ScriptReference/Application.OpenURL.html) for behaviour details and limitations. 
    /// </remarks>
    public class OpenURL : Command
    {
        /// <summary>
        /// URL to open.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter URL;

        public override UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            Application.OpenURL(URL);
            return UniTask.CompletedTask;
        }
    }
}
