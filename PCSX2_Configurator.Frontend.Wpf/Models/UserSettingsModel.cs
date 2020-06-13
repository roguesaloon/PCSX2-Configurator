using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public class UserSettingsModel : INotifyPropertyChanged
    {
        IDictionary<string, object> userSettings;
        public UserSettingsModel(IDictionary<string, object> userSettings)
        {
            this.userSettings = userSettings;
        }

        public bool AutoApplyRemoteConfigs 
        { 
            get => (bool) Convert.ChangeType(userSettings[nameof(AutoApplyRemoteConfigs)], typeof(bool)); 
            set => userSettings[nameof(AutoApplyRemoteConfigs)] = value; 
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
