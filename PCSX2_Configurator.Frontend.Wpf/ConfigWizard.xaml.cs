using System.Windows;
using System.IO;
using PCSX2_Configurator.Core;
using static PCSX2_Configurator.Core.ConfigurationService;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for ConfigWizard.xaml
    /// </summary>
    public partial class ConfigWizard : Window
    {
        private readonly ConfigurationService configurationService;
        private readonly SettingsService settingsService;
        private readonly GameModel gameModel;

        public ConfigWizard(ConfigurationService configurationService, SettingsService settingsService, GameModel gameModel)
        {
            InitializeComponent();
            this.configurationService = configurationService;
            this.settingsService = settingsService;
            this.gameModel = gameModel;
            version.ItemsSource = GameModel.Versions;
            version.SelectedItem = gameModel.Version;
        }

        private void CreateConfig(object sender, RoutedEventArgs e)
        {
            var selectedVersion = version.SelectedItem as string;
            var givenName = configName.Text;
            if (string.IsNullOrWhiteSpace(givenName) || string.IsNullOrWhiteSpace(selectedVersion))
            {
                var error = string.IsNullOrWhiteSpace(givenName) ? "enter a name" : "choose a version";
                MessageBox.Show("You must " + error, "Error");
                return;
            }
            var inisPath = $"{Path.GetDirectoryName(settingsService.VersionsAndPaths[selectedVersion])}\\inis";
            inisPath = selectedVersion.Contains("1.4.0") ? inisPath.Replace("\\inis", "\\inis_1.4.0") : inisPath;
            configurationService.CreateConfig(givenName, inisPath, SettingsOptions.All);
            settingsService.UpdateAvailableConfigs();
            GameModel.Configs = settingsService.AvalialableConfigs.Keys;
            gameModel.Version = selectedVersion;
            gameModel.Config = givenName;
            Close();
        }
    }
}
