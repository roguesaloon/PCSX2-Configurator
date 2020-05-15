using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PCSX2_Configurator.Core;
using PCSX2_Configurator.Settings;
using static PCSX2_Configurator.Core.ConfigOptions;

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
            configName.Text = gameModel.Game.ToLowerInvariant().Replace(" - ", "-").Replace(" ", "-");
            configName.Text = FileHelpers.GetFileNameSafeString(configName.Text);
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

        private void OnlyAcceptNumbers(object sender, TextCompositionEventArgs e) =>
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]");

        private ConfigOptions GetConfigOptions()
        {
            var options = new ConfigOptions();

            if (options_Logs.IsChecked.Value) options.Flags |= ConfigFlags.CopyLogSettings;
            if (options_Folders.IsChecked.Value) options.Flags |= ConfigFlags.CopyFolderSettings;
            if (options_Files.IsChecked.Value) options.Flags |= ConfigFlags.CopyFileSettings;
            if (options_Window.IsChecked.Value) options.Flags |= ConfigFlags.CopyWindowSettings;
            if (options_VM.IsChecked.Value) options.Flags |= ConfigFlags.CopyVmSettings;
            if (options_GSdx.IsChecked.Value) options.Flags |= ConfigFlags.CopyGsdxSettings;
            if (options_SPU2X.IsChecked.Value) options.Flags |= ConfigFlags.CopySpu2xSettings;
            if (options_LilyPad.IsChecked.Value) options.Flags |= ConfigFlags.CopyLilyPadSettings;
            if (options_NoPresets.IsChecked.Value) options.Flags |= ConfigFlags.DisablePresets;
            if (options_GameFixes.IsChecked.Value) options.Flags |= ConfigFlags.EnableGameFixes;
            if (options_SpeedHacks.IsChecked.Value) options.Flags |= ConfigFlags.EnableSpeedHacks;
            if (options_WidescreenPatches.IsChecked.Value) options.Flags |= ConfigFlags.EnableWidescreenPatches;
            if (options_Cheats.IsChecked.Value) options.Flags |= ConfigFlags.EnableCheats;

            var aspectRatio = (options_AspectRatio.SelectedItem as ComboBoxItem).Content as string;
            aspectRatio = Regex.Replace(aspectRatio, @" ?\(.*?\)", string.Empty).Trim();
            options.AspectRatio = (ConfigAspectRatio) Enum.Parse(typeof(ConfigAspectRatio), aspectRatio);
            options.ZoomLevel = int.Parse(options_Zoom.Text);

            return options;
        }
    }
}
