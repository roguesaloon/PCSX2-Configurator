using System.Windows;
using System.Collections.Generic;
using MoreLinq;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Services;


namespace PCSX2_Configurator.Frontend.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for RemoteConfigImporter.xaml
    /// </summary>
    public partial class RemoteConfigImporter : Window
    {
        private readonly IRemoteConfigService remoteConfigService;
        public RemoteConfigImporter(IRemoteConfigService remoteConfigService)
        {
            this.remoteConfigService = remoteConfigService;
            InitializeComponent();
        }

        public void Show(IEnumerable<GameInfo> gameInfos)
        {
            configSelection.ItemsSource = remoteConfigService.AvailableConfigs;
            gameInfos.ForEach(info => gameSelection.Items.Add($"{info.DisplayName} | {info.GameId}"));
            Show();
        }
    }
}
