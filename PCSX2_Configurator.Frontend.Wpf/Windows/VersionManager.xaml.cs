using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using PCSX2_Configurator.Settings;
using PCSX2_Configurator.Services;

namespace PCSX2_Configurator.Frontend.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for VersionManager.xaml
    /// </summary>
    public partial class VersionManager : Window
    {
        private readonly IVersionManagementService versionManagementService;
        private IDictionary<string, VersionSettings> availableVersions;
        public VersionManager(IVersionManagementService versionManagementService)
        {
            this.versionManagementService = versionManagementService;
            InitializeComponent();
        }

        public new async void Show()
        {
            availableVersions = await versionManagementService.GetAvailableVersions();
            versionSelector.ItemsSource = availableVersions.Keys;
            base.Show();
        }

        private async void InstallVersion(object sender, RoutedEventArgs e)
        {
            if (!(versionSelector.SelectedItem is string selectedVersion)) return;

            Mouse.OverrideCursor = Cursors.Wait;
            await versionManagementService.InstallVersion(availableVersions[selectedVersion]);
            GameModel.Versions = GameModel.Versions;
            Mouse.OverrideCursor = null;
            Close();
        }
    }
}
