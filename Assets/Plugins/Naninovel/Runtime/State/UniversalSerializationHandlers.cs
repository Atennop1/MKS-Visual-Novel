// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#if UNITY_EDITOR || !(UNITY_SWITCH || UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE)
#define IO_SERIALIZER_SUPPORTED
#endif

namespace Naninovel
{
    /// <summary>
    /// Will use async IO handlers on supported platforms and fallback to PlayerPrefs.
    /// </summary>
    public class UniversalGameStateSerializer
        #if IO_SERIALIZER_SUPPORTED
        : IOGameStateSlotManager
        #else
        : PlayerPrefsGameStateSlotManager
        #endif
    {
        public UniversalGameStateSerializer (StateConfiguration config, string savesFolderPath)
            : base(config, savesFolderPath) { }
    }

    /// <inheritdoc cref="UniversalGameStateSerializer"/>
    public class UniversalGlobalStateSerializer
        #if IO_SERIALIZER_SUPPORTED
        : IOGlobalStateSlotManager
        #else
        : PlayerPrefsGlobalStateSlotManager
        #endif
    {
        public UniversalGlobalStateSerializer (StateConfiguration config, string savesFolderPath)
            : base(config, savesFolderPath) { }
    }

    /// <inheritdoc cref="UniversalGameStateSerializer"/>
    public class UniversalSettingsStateSerializer
        #if IO_SERIALIZER_SUPPORTED
        : IOSettingsSlotManager
        #else
        : PlayerPrefsSettingsSlotManager
        #endif
    {
        public UniversalSettingsStateSerializer (StateConfiguration config, string savesFolderPath)
            : base(config, savesFolderPath) { }
    }
}
