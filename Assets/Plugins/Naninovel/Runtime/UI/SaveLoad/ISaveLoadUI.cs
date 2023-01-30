// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    /// <summary>
    /// Represents a set of UI elements for managing <see cref="GameStateMap"/> slots.
    /// </summary>
    /// <remarks>
    /// Implementation is expected to have two independent sets of slots: `normal` and `quick`.
    /// </remarks>
    public interface ISaveLoadUI : IManagedUI
    {
        /// <summary>
        /// Current presentation mode of the UI.
        /// </summary>
        SaveLoadUIPresentationMode PresentationMode { get; set; }

        /// <summary>
        /// Returns either <see cref="SaveLoadUIPresentationMode.Load"/> or <see cref="SaveLoadUIPresentationMode.QuickLoad"/>, 
        /// depending on where is the latest (chronologically, based on <see cref="GameStateMap.SaveDateTime"/>) save slot resides.
        /// </summary>
        SaveLoadUIPresentationMode GetLastLoadMode ();
    }
}
