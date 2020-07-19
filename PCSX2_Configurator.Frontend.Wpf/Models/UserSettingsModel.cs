#pragma warning disable 0067

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class UserSettingsModel : INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> userSettings;
        public UserSettingsModel(IDictionary<string, object> userSettings)
        {
            this.userSettings = userSettings;
        }

        public bool AutoApplyRemoteConfigs
        {
            get => (bool)Convert.ChangeType(userSettings[nameof(AutoApplyRemoteConfigs)], typeof(bool));
            set => userSettings[nameof(AutoApplyRemoteConfigs)] = value;
        }

        public string DefaultPcsx2Version
        {
            get => (string)Convert.ChangeType(userSettings[nameof(DefaultPcsx2Version)], typeof(string));
            set => userSettings[nameof(DefaultPcsx2Version)] = value;
        }

        private static List<string> AvailablePcsx2Versions
        {
            get
            {
                var availablePcsx2Versions = new List<string>
                {
                    "Use Latest Stable Version",
                    "Use Latest Version"
                };
                availablePcsx2Versions.AddRange(GameModel.Versions);
                return availablePcsx2Versions;
            }
        }
        public IEnumerable<Tuple<string, bool>> DefaultPcsx2VersionSelection => AvailablePcsx2Versions.Select(version => new Tuple<string, bool>(version, DefaultPcsx2Version == version));

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
