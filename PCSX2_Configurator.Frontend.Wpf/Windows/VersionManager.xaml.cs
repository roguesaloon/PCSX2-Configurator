using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using PCSX2_Configurator.Core;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for VersionManager.xaml
    /// </summary>
    public partial class VersionManager : Window
    {
        private readonly VersionManagementService versionManagementService;
        private IDictionary<string, VersionSettings> availableVersions;
        public VersionManager(VersionManagementService versionManagementService)
        {
            this.versionManagementService = versionManagementService;
            InitializeComponent();
        }

        new public async void Show()
        {
            availableVersions = await versionManagementService.GetAvailableVersions();
            versionSelector.ItemsSource = availableVersions.Keys;
            base.Show();
        }

        private async void InstallVersion(object sender, RoutedEventArgs e)
        {
            var selectedVersion = versionSelector.SelectedItem as string;

            Mouse.OverrideCursor = Cursors.Wait;
            await versionManagementService.InstallVersion(availableVersions[selectedVersion]);
            GameModel.Versions = GameModel.Versions;
            Mouse.OverrideCursor = null;

            Close();
        }
    }
}
