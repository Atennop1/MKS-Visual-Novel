// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    /// <summary>
    /// Represents a set of UI elements used for browsing unlockable tips.
    /// </summary>
    public interface ITipsUI : IManagedUI
    {
        /// <summary>
        /// Number of existing unlockable tip items, independent of the unlock state.
        /// </summary>
        int TipsCount { get; }

        /// <summary>
        /// Selects tip record with the provided ID.
        /// </summary>
        /// <param name="tipId">ID of the tip to select.</param>
        void SelectTipRecord (string tipId);
    }
}
