// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.SceneManagement;

namespace Naninovel.Commands
{
    /// <summary>
    /// Unloads a [Unity scene](https://docs.unity3d.com/Manual/CreatingScenes.html) with the provided name.
    /// Don't forget to add the required scenes to the [build settings](https://docs.unity3d.com/Manual/BuildSettings.html) to make them available for loading.
    /// Be aware, that only scenes loaded additively can be then unloaded (at least one scene should always remain loaded).
    /// </summary>
    public class UnloadScene : Command
    {
        /// <summary>
        /// Name of the scene to unload.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter SceneName;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            await SceneManager.UnloadSceneAsync(SceneName);
        }
    }
}
