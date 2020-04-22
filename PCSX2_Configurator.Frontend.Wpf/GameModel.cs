using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class GameModel : INotifyPropertyChanged
    {
        public string Game { get; set; }
        public string Path { get; set; }
        public string CoverPath { get; set; }

        public static IEnumerable<string> Versions { get; set; }
        public static IEnumerable<string> Configs { get; set; }

        public IEnumerable<Tuple<string, bool>> VersionsAndStates => Versions.Select(version => new Tuple<string, bool>(version, Version == version));
        public IEnumerable<Tuple<string, bool>> ConfigsAndStates => Configs.Select(config => new Tuple<string, bool>(config, Config == config));

        [AlsoNotifyFor(nameof(VersionsAndStates))] public string Version { get; set; }
        [AlsoNotifyFor(nameof(ConfigsAndStates))] public string Config { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
    }
}