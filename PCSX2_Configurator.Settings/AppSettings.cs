using System;
using System.Collections.Generic;
using System.IO;

namespace PCSX2_Configurator.Settings
{
    public class AppSettings
    {
        public string ConfigsDirectory { get; private set; }
        public Dictionary<string, string> Versions { get; private set; }
        public string GameLibraryFile { get; private set; }
        public Covers Covers { get; private set; }

        private Dictionary<string, string> configs;
        public Dictionary<string, string> Configs
        {
            get
            {
                if (configs == null)
                {
                    configs = new Dictionary<string, string>();
                    var directories = Directory.GetDirectories(ConfigsDirectory);
                    foreach (var directory in directories)
                        configs.Add(new DirectoryInfo(directory).Name, directory);
                }

                return configs;
            }
        }
        public void UpdateConfigs() => configs = null;
    }
}
