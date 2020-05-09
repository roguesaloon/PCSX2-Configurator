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
            var options = GetConfigOptions();
            configurationService.CreateConfig(givenName, inisPath, options);
            settings.UpdateConfigs();
            GameModel.Configs = settings.Configs.Keys;
            gameModel.Version = selectedVersion;
            gameModel.Config = givenName;
            Close();
        }

        private ConfigOptions GetConfigOptions()
        {
            var options = ConfigOptions.None;
            if (options_Logs.IsChecked.Value) options |= ConfigOptions.CopyLogSettings;
            if (options_Folders.IsChecked.Value) options |= ConfigOptions.CopyFolderSettings;
            if (options_Files.IsChecked.Value) options |= ConfigOptions.CopyFileSettings;
            if (options_Window.IsChecked.Value) options |= ConfigOptions.CopyWindowSettings;
            if (options_VM.IsChecked.Value) options |= ConfigOptions.CopyVmSettings;
            if (options_GSdx.IsChecked.Value) options |= ConfigOptions.CopyGsdxSettings;
            if (options_SPU2X.IsChecked.Value) options |= ConfigOptions.CopySpu2xSettings;
            if (options_LilyPad.IsChecked.Value) options |= ConfigOptions.CopyLilyPadSettings;
            if (options_NoPresets.IsChecked.Value) options |= ConfigOptions.DisablePresets;
            if (options_GameFixes.IsChecked.Value) options |= ConfigOptions.EnableGameFixes;
            if (options_SpeedHacks.IsChecked.Value) options |= ConfigOptions.EnableSpeedHacks;
            return options;
        }
    }
}
