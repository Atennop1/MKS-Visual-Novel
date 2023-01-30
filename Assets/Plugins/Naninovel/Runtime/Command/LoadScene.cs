// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine.SceneManagement;

namespace Naninovel.Commands
{
    /// <summary>
    /// Loads a [Unity scene](https://docs.unity3d.com/Manual/CreatingScenes.html) with the provided name.
    /// Don't forget to add the required scenes to the [build settings](https://docs.unity3d.com/Manual/BuildSettings.html) to make them available for loading.
    /// </summary>
    public class LoadScene : Command
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter SceneName;
        /// <summary>
        /// Whether to load the scene additively, or unload any currently loaded scenes before loading the new one (default).
        /// See the [load scene documentation](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html) for more information.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Additive = false;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            await SceneManager.LoadSceneAsync(SceneName, Additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }
    }
}
