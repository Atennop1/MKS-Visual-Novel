// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the game save and load events invoked by <see cref="IStateManager"/>. 
    /// </summary>
    public class GameSaveLoadArgs : EventArgs
    {
        /// <summary>
        /// ID of the save slot the operation is associated with.
        /// </summary>
        public readonly string SlotId;
        /// <summary>
        /// Whether it's a quick save/load operation.
        /// </summary>
        public readonly bool Quick;

        public GameSaveLoadArgs (string slotId, bool quick)
        {
            SlotId = slotId;
            Quick = quick;
        }
    }
}