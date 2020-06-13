using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class GameModel : INotifyPropertyChanged
    {
        private readonly static List<GameModel> gameModels = new List<GameModel>();
        public GameModel() => gameModels.Add(this);

        public GameInfo GameInfo { get; set; }

        public string Game { get => GameInfo?.DisplayName ?? GameInfo?.Name; }
        public string Path { get; set; }
        public string CoverPath { get; set; }
        public string LaunchOptions { get; set; }

        private static IEnumerable<string> versions = new List<string>();
        public static IEnumerable<string> Versions
        {
            get => versions;
            set
            {
                versions = value;
                foreach (var model in gameModels)
                {
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(VersionsAndStates)));
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(HasVersions)));
                }
            }
        }

        private static IEnumerable<ConfigModel> configModels = new List<ConfigModel>();
        private static IEnumerable<string> configs = new List<string>();
        public static IEnumerable<string> Configs
        {
            get => configs;
            set
            {
                configs = value;
                configModels = configs.Select(name => new ConfigModel(name));
                foreach (var model in gameModels)
                {
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(ConfigsAndStates)));
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(HasConfigs)));
                }
            }
        }

        private IEnumerable<ConfigModel> FilteredConfigs => configModels.Where(config => config.GameIds != null && config.GameIds.Contains(GameInfo.GameId) || config.GameIds == null);
        public IEnumerable<Tuple<string, bool>> VersionsAndStates => Versions.Select(version => new Tuple<string, bool>(version, Version == version));
        public IEnumerable<Tuple<string, bool>> ConfigsAndStates => FilteredConfigs.Select(config => new Tuple<string, bool>(config.Name, Config == config.Name));

        public bool HasVersions => Versions.Count() > 0;
        public bool HasConfigs => FilteredConfigs.Count() > 0;
        public string RemoteConfig => FilteredConfigs.FirstOrDefault(config => config.IsRemote).Name;

        public bool HasConfig => Config != null && FilteredConfigs.Select(config => config.Name).Contains(Config);

        public string Version { get; set; }
        public string Config { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}