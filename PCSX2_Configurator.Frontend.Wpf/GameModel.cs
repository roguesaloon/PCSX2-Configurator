using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class GameModel : INotifyPropertyChanged
    {
        private readonly static List<GameModel> gameModels = new List<GameModel>();
        public GameModel() => gameModels.Add(this);

        public string Game { get; set; }
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

        private static IEnumerable<string> configs = new List<string>();
        public static IEnumerable<string> Configs
        {
            get => configs;
            set
            {
                configs = value;
                foreach (var model in gameModels)
                {
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(ConfigsAndStates)));
                    model.PropertyChanged?.Invoke(model, new PropertyChangedEventArgs(nameof(HasConfigs)));
                }
            }
        }

        public IEnumerable<Tuple<string, bool>> VersionsAndStates => Versions.Select(version => new Tuple<string, bool>(version, Version == version));
        public IEnumerable<Tuple<string, bool>> ConfigsAndStates => Configs.Select(config => new Tuple<string, bool>(config, Config == config));

        public bool HasVersions => VersionsAndStates.Count() > 0;
        public bool HasConfigs => ConfigsAndStates.Count() > 0;

        public bool HasConfig => Config != null && Configs.Contains(Config);

        public string Version { get; set; }
        public string Config { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}