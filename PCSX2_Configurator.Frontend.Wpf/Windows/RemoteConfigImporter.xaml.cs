using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Services;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Frontend.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for RemoteConfigImporter.xaml
    /// </summary>
    public partial class RemoteConfigImporter : Window
    {
        private readonly IRemoteConfigService remoteConfigService;
        private readonly AppSettings settings;
        private Func<string> versionSelector;
        public RemoteConfigImporter(AppSettings settings, IRemoteConfigService remoteConfigService)
        {
            this.settings = settings;
            this.remoteConfigService = remoteConfigService;
            InitializeComponent();
        }

        public void Show(IEnumerable<GameInfo> gameInfos, Func<string> versionSelector)
        {
            this.versionSelector = versionSelector;
            configSelection.ItemsSource = remoteConfigService.AvailableConfigs.OrderBy(config => config);
            gameInfos.OrderBy(info => info.DisplayName).ForEach(info => gameSelection.Items.Add(new Tuple<string, GameInfo>($"{info.DisplayName} | {info.GameId}", info)));
            gameSelection.DisplayMemberPath = "Item1";
            Show();
        }

        private void ShouldImportForAll(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox.IsChecked.Value)
            {
                gameSelection.UnselectAll();
                gameSelection.IsEnabled = false;
            }
            else if(!gameSelection.IsEnabled) gameSelection.IsEnabled = true;
        }

        private void ImportConfig(object sender, RoutedEventArgs e)
        {
            var configName = configSelection.Text;
            if(string.IsNullOrWhiteSpace(configName))
            {
                MessageBox.Show("No Config Selected", "Error");
                return;
            }

            if(gameSelection.SelectedItems.Count == 0 && !importForAll.IsChecked.Value)
            {
                MessageBox.Show("No Game(s) Selected to Import Config to", "Error");
                return;
            }

            var gameIds = Array.Empty<string>();
            gameIds = !importForAll.IsChecked.Value ? gameSelection.SelectedItems.Cast<Tuple<string, GameInfo>>().Select(selection => selection.Item2.GameId).ToArray() : gameIds;

            var versionToUse = versionSelector?.Invoke();
            if(versionToUse == null)
            {
                MessageBox.Show("No PCSX2 Version is Installed", "Error");
                return;
            }

            var emulatorPath = settings.Versions[versionToUse];
            remoteConfigService.ImportConfig(configName, emulatorPath, gameIds);
            GameModel.Configs = settings.Configs.Keys;
            Close();
        }
    }
}
