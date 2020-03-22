using System;
using System.Windows;
using Path = System.IO.Path;
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

        public ConfigWizard(ConfigurationService configurationService, SettingsService settingsService, GameModel gameModel, Action onClose)
        {
            InitializeComponent();
            this.configurationService = configurationService;
            this.settingsService = settingsService;
            this.gameModel = gameModel;
            Closing += (sender, e) => onClose();
        }

        private void CreateConfig(object sender, RoutedEventArgs e)
        {
            var inisPath = $"{Path.GetDirectoryName(settingsService.VersionsAndPaths[gameModel.Version])}\\inis";
            configurationService.CreateConfig(configName.Text, inisPath, SettingsOptions.All);
            settingsService.UpdateAvailableConfigs();
            Close();
        }
    }
}
