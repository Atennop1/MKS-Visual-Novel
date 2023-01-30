// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Sets an [unlockable item](/guide/unlockable-items.md) with the provided ID to `locked` state.
    /// </summary>
    /// <remarks>
    /// The unlocked state of the items is stored in [global scope](/guide/state-management.md#global-state).<br/>
    /// In case item with the provided ID is not registered in the global state map, 
    /// the corresponding record will automatically be added.
    /// </remarks>
    public class Lock : Command, Command.IForceWait
    {
        /// <summary>
        /// ID of the unlockable item. Use `*` to lock all the registered unlockable items. 
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ResourceContext(UnlockablesConfiguration.DefaultPathPrefix)]
        public StringParameter Id;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var unlockableManager = Engine.GetService<IUnlockableManager>();

            if (Id.Value.EqualsFastIgnoreCase("*")) unlockableManager.LockAllItems();
            else unlockableManager.LockItem(Id);

            await Engine.GetService<IStateManager>().SaveGlobalAsync();
        }
    } 
}
