﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PCSX2_Configurator.Settings
{
    public class AppSettings
    {
        private string configsDirectory;
        public string ConfigsDirectory { get => configsDirectory; private set => configsDirectory = Path.GetFullPath(value); }

        private string remoteConfigsPath;
        public string RemoteConfigsPath { get => remoteConfigsPath; private set => remoteConfigsPath = Path.GetFullPath(value); }

        private string gameLibraryFile;
        public string GameLibraryFile { get => gameLibraryFile; private set => gameLibraryFile = Path.GetFullPath(value); }

        private string SevenZipx86_LibraryPath { get;  set; }
        private string SevenZipx64_LibraryPath { get; set; }
        public string SevenZipLibraryPath { get => Path.GetFullPath(Environment.Is64BitProcess ? SevenZipx64_LibraryPath : SevenZipx86_LibraryPath); }

        private string Gitx86_LibraryPath { get; set; }
        private string Gitx64_LibraryPath { get; set; }
        public string GitLibraryPath { get => Path.GetFullPath(Environment.Is64BitProcess ? Gitx64_LibraryPath : Gitx86_LibraryPath); }

        private string autoHotkeyExecutable;
        public string AutoHotkeyExecutable { get => autoHotkeyExecutable; private set => autoHotkeyExecutable = Path.GetFullPath(value); }

        private string autoHotkeyScript;
        public string AutoHotkeyScript { get => autoHotkeyScript; private set => autoHotkeyScript = Path.GetFullPath(value); }

        private string additionalPluginsDirectory;
        public string AdditionalPluginsDirectory { get => additionalPluginsDirectory; private set => additionalPluginsDirectory = Path.GetFullPath(value); }

        private string compressedMemCard;
        public string CompressedMemCard { get => compressedMemCard; private set => compressedMemCard = Path.GetFullPath(value); }

        public string DefaultLaunchOptions { get; private set; }
        public CoverSettings Covers { get; private set; }
        public VersionManagerSettings VersionManager { get; private set; }
        public IntPtr AutoHotkeyWindowHandle { get; private set; }

        public Dictionary<string, string> Versions { get; private set; } = new Dictionary<string, string>();
        public async Task UpdateVersions()
        {
            var settingsJson = await File.ReadAllTextAsync("settings.json");
            var settingsObj = JsonConvert.DeserializeObject<JObject>(settingsJson);
            settingsObj[nameof(Versions)] = JToken.FromObject(Versions);
            settingsJson = JsonConvert.SerializeObject(settingsObj, Formatting.Indented);
            await File.WriteAllTextAsync("settings.json", settingsJson);
        }

        public Dictionary<string, object> UserSettings { get; private set; } = new Dictionary<string, object>();
        public async Task UpdateUserSettings()
        {
            var settingsJson = await File.ReadAllTextAsync("settings.json");
            var settingsObj = JsonConvert.DeserializeObject<JObject>(settingsJson);

            // Transform string typed user settings to strong types
            UserSettings = UserSettings.ToDictionary(setting => setting.Key, setting => {
                var value = setting.Value;
                value = (object) value == "True"  ? true  : value;
                value = (object) value == "False" ? false : value;
                return value;
            });

            settingsObj[nameof(UserSettings)] = JToken.FromObject(UserSettings);
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
