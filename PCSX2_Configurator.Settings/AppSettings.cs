using System.Collections.Generic;
using System.IO;

namespace PCSX2_Configurator.Settings
{
    public class AppSettings
    {
        public string ConfigsDirectory { get; private set; }
        public string RemoteConfigsPath { get; private set; } 
        public Dictionary<string, string> Versions { get; private set; } = new Dictionary<string, string>();
        public string GameLibraryFile { get; private set; }
        public string SevenZipLibraryPath { get; private set; }
        public CoverSettings Covers { get; private set; }
        public VersionManagerSettings VersionManager { get; private set; }
        
        private Dictionary<string, string> configs;
        public Dictionary<string, string> Configs
        {
            get
            {
                if (configs == null)
                {
                    configs = new Dictionary<string, string>();
                    if (Directory.Exists(ConfigsDirectory))
                    {
                        var directories = Directory.GetDirectories(ConfigsDirectory);
                        foreach (var directory in directories)
                            configs.Add(new DirectoryInfo(directory).Name, directory);
                    }
                }

                return configs;
            }
        }
        public void UpdateConfigs() => configs = null;
    }
}
