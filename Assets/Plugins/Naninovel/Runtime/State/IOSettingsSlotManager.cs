// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.IO;

namespace Naninovel
{
    public class IOSettingsSlotManager : IOSaveSlotManager<SettingsStateMap>
    {
        protected override string SaveDataPath => $"{GameDataPath}/{savesFolderPath}";
        protected override string Extension => Binary ? "nson" : "json";
        protected override bool Binary => false;

        private readonly string defaultSlotId;
        private readonly string savesFolderPath;

        public IOSettingsSlotManager (StateConfiguration config, string savesFolderPath) 
        {
            this.savesFolderPath = savesFolderPath;
            defaultSlotId = config.DefaultSettingsSlotId;
        }

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            return Directory.GetFiles(SaveDataPath, $"{defaultSlotId}.{Extension}", SearchOption.TopDirectoryOnly).Length > 0;
        }
    }
}
