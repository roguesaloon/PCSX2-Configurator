using IniParser;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCSX2_Configurator.Core
{
    public sealed class SettingsService
    {
        public Dictionary<string, string> VersionsAndPaths { get; private set; }
        public string ConfigsDir { get; private set; }
        private Dictionary<string, string> availableConfigs;
        public Dictionary<string, string> AvalialableConfigs
        {
            get
            {
                if(availableConfigs == null)
                {
                    availableConfigs = new Dictionary<string, string>();
                    var directories = Directory.GetDirectories(ConfigsDir);
                    foreach (var directory in directories)
                        availableConfigs.Add(new DirectoryInfo(directory).Name, directory);
                }

                return availableConfigs;
            }
        }

        private readonly FileIniDataParser iniParser;
        private readonly string settingsFilePath;

        public SettingsService(string filePath)
        {
            iniParser = new FileIniDataParser();
            settingsFilePath = filePath ?? "Settings.ini";
            LoadSettingsFromFile();
        }

        public void LoadSettingsFromFile()
        {
            var settings = iniParser.ReadFile(settingsFilePath);
            VersionsAndPaths = settings["PCSX2_Versions"].ToDictionary(x => x.KeyName, x => x.Value);
            ConfigsDir = settings["PCSX2_Configurator"]["ConfigsDir"];
        }

        public void UpdateAvailableConfigs()
        {
            availableConfigs = null;
            availableConfigs = AvalialableConfigs;
        }
    }
}
