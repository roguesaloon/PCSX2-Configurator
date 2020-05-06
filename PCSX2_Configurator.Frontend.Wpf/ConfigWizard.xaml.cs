using System.Windows;
using PCSX2_Configurator.Core;
using PCSX2_Configurator.Settings;
using static PCSX2_Configurator.Core.ConfigurationService;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for ConfigWizard.xaml
    /// </summary>
    public partial class ConfigWizard : Window
    {
        private readonly AppSettings settings;
        private readonly ConfigurationService configurationService;
        private GameModel gameModel;

        public ConfigWizard(AppSettings settings, ConfigurationService configurationService)
        {
            InitializeComponent();
            this.configurationService = configurationService;
            this.settings = settings;
        }

        public void Show(GameModel gameModel)
        {
            this.gameModel = gameModel;
            version.ItemsSource = GameModel.Versions;
            version.SelectedItem = gameModel.Version;
            Show();
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
            var inisPath = EmulationService.GetInisPath(settings.Versions[selectedVersion]);
            configurationService.CreateConfig(givenName, inisPath, SettingsOptions.All);
            settings.UpdateConfigs();
            GameModel.Configs = settings.Configs.Keys;
            gameModel.Version = selectedVersion;
            gameModel.Config = givenName;
            Close();
        }
    }
}
