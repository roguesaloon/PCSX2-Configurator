using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PCSX2_Configurator.Settings
{
    public class AppSettings
    {
        public string ConfigsDirectory { get; private set; }
        public string RemoteConfigsPath { get; private set; } 
        public string GameLibraryFile { get; private set; }
        public string SevenZipLibraryPath { get; private set; }
        public string AdditionalPluginsDirectory { get; private set; }
        public CoverSettings Covers { get; private set; }
        public VersionManagerSettings VersionManager { get; private set; }

        public Dictionary<string, string> Versions { get; private set; } = new Dictionary<string, string>();
        public async Task UpdateVersions()
        {
            var settingsJson = await File.ReadAllTextAsync("settings.json");
            var settingsObj = JsonConvert.DeserializeObject<JObject>(settingsJson);
            settingsObj[nameof(Versions)] = JToken.FromObject(Versions);
            settingsJson = JsonConvert.SerializeObject(settingsObj, Formatting.Indented);
            await File.WriteAllTextAsync("settings.json", settingsJson);
        }


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
