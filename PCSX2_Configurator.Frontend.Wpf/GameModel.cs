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
        public static IEnumerable<string> Versions { get; set; }
        public IEnumerable<Tuple<string, bool>> VersionsAndStates => Versions.Select(version => new Tuple<string, bool>(version, Version == version));
        private string version;
        public string Version 
        {
            get => version;
            set
            {
                version = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VersionsAndStates)));
            }
        }
        public static IEnumerable<string> Configs { get; set; }
        public IEnumerable<Tuple<string, bool>> ConfigsAndStates => Configs.Select(config => new Tuple<string, bool>(config, Config == config));
        private string config;
        public string Config 
        {
            get => config;
            set
            {
                config = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Config)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfigsAndStates)));
            }
        }
        public string coverPath;
        public string CoverPath 
        {
            get => coverPath; 
            set
            {
                coverPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoverPath)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}