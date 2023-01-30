// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    [EditInProjectSettings]
    public class StateConfiguration : Configuration
    {
        [Tooltip("The folder will be created in the game data folder.")]
        public string SaveFolderName = "Saves";
        [Tooltip("The name of the settings save file.")]
        public string DefaultSettingsSlotId = "Settings";
        [Tooltip("The name of the global save file.")]
        public string DefaultGlobalSlotId = "GlobalSave";
        [Tooltip("Mask used to name save slots.")]
        public string SaveSlotMask = "GameSave{0:000}";
        [Tooltip("Mask used to name quick save slots.")]
        public string QuickSaveSlotMask = "GameQuickSave{0:000}";
        [Tooltip("Maximum number of save slots."), Range(1, 999)]
        public int SaveSlotLimit = 99;
        [Tooltip("Maximum number of quick save slots."), Range(1, 999)]
        public int QuickSaveSlotLimit = 18;
        [Tooltip("Whether to compress and store the saves as binary files (.nson) instead of text files (.json). This will significantly reduce the files size and make them harder to edit (to prevent cheating), but will consume more memory and CPU time when saving and loading.")]
        public bool BinarySaveFiles = true;
        [Tooltip("Seconds to wait before starting load operations; used to allow pre-load animations to complete before any load-related stutters could happen.")]
        public float LoadStartDelay = 0.3f;
        [Tooltip("Whether to reset state of the engine services when loading another script via [@goto] command. Can be used instead of [@resetState] command to automatically unload all the resources on each goto.")]
        public bool ResetOnGoto = false;

        [Header("State Rollback")]
        [Tooltip("Whether to enable state rollback feature allowing player to rewind the script backwards.")]
        public bool EnableStateRollback = true;
        [Tooltip("The number of state snapshots to keep at runtime; determines how far back the rollback (rewind) can be performed. Increasing this value will consume more memory.")]
        public int StateRollbackSteps = 1024;
        [Tooltip("The number of state snapshots to serialize (save) under the save game slots; determines how far back the rollback can be performed after loading a saved game. Increasing this value will enlarge save game files.")]
        public int SavedRollbackSteps = 128;

        [Header("Serialization Handlers")]
        [Tooltip("Implementation responsible for de-/serializing local (session-specific) game state; see `State Management` guide on how to add custom serialization handlers.")]
        public string GameStateHandler = typeof(UniversalGameStateSerializer).AssemblyQualifiedName;
        [Tooltip("Implementation responsible for de-/serializing global game state; see `State Management` guide on how to add custom serialization handlers.")]
        public string GlobalStateHandler = typeof(UniversalGlobalStateSerializer).AssemblyQualifiedName;
        [Tooltip("Implementation responsible for de-/serializing game settings; see `State Management` guide on how to add custom serialization handlers.")]
        public string SettingsStateHandler = typeof(UniversalSettingsStateSerializer).AssemblyQualifiedName;

        /// <summary>
        /// Generates save slot ID using provided index and <see cref="SaveSlotMask"/>.
        /// </summary>
        public string IndexToSaveSlotId (int index) => string.Format(SaveSlotMask, index);
        /// <summary>
        /// Generates quick save slot ID using provided index and <see cref="QuickSaveSlotMask"/>.
        /// </summary>
        public string IndexToQuickSaveSlotId (int index) => string.Format(QuickSaveSlotMask, index);
    }
}
