// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel.Commands
{
    /// <summary>
    /// Resets state of the [engine services](https://naninovel.com/guide/engine-services.html) and unloads (disposes) 
    /// all the resources loaded by Naninovel (textures, audio, video, etc); will basically revert to an empty initial engine state.
    /// </summary>
    /// <remarks>
    /// The process is asynchronous and is masked with a loading screen ([ILoadingUI](https://naninovel.com/guide/user-interface.html#ui-customization)).
    /// <br/><br/>
    /// Be aware, that this command can not be undone (rewound back).
    /// </remarks>
    public class ResetState : Command, Command.IForceWait
    {
        /// <summary>
        /// Names of the [engine services](https://naninovel.com/guide/engine-services.html) (interfaces) to exclude from reset.
        /// Consider adding `ICustomVariableManager` to preserve the local variables.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias)]
        public StringListParameter Exclude;
        /// <summary>
        /// Names of the [engine services](https://naninovel.com/guide/engine-services.html) (interfaces) to reset;
        /// other services won't be affected. Doesn't have effect when the nameless (exclude) parameter is assigned.
        /// </summary>
        public StringListParameter Only;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            if (Assigned(Exclude)) await Engine.GetService<IStateManager>().ResetStateAsync(Exclude);
            else if (Assigned(Only))
            {
                var serviceTypes = Engine.FindAllServices<IEngineService>().Select(s => s.GetType()).ToArray();
                var onlyTypeNames = Only.Value.Select(v => v.Value);
                var onlyTypes = serviceTypes.Where(t => onlyTypeNames.Any(ot => ot == t.Name || t.GetInterface(ot) != null));
                var excludeTypes = serviceTypes.Where(t => !onlyTypes.Any(ot => ot.IsAssignableFrom(t))).ToArray();
                await Engine.GetService<IStateManager>().ResetStateAsync(excludeTypes);
            }
            else await Engine.GetService<IStateManager>().ResetStateAsync();
        }
    }
}
