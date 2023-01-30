// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.IO;

namespace Naninovel
{
    public class IOGlobalStateSlotManager : IOSaveSlotManager<GlobalStateMap>
    {
        protected override string SaveDataPath => $"{GameDataPath}/{savesFolderPath}";
        protected override string Extension => Binary ? "nson" : "json";
        protected override bool Binary => config.BinarySaveFiles;

        private readonly string defaultSlotId;
        private readonly string savesFolderPath;
        private readonly StateConfiguration config;

        public IOGlobalStateSlotManager (StateConfiguration config, string savesFolderPath) 
        {
            this.savesFolderPath = savesFolderPath;
            this.config = config;
            defaultSlotId = config.DefaultGlobalSlotId;
        }

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            return Directory.GetFiles(SaveDataPath, $"{defaultSlotId}.{Extension}", SearchOption.TopDirectoryOnly).Length > 0;
        }
    }
}
