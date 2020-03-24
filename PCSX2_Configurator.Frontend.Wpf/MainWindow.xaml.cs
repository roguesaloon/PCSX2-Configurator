using PCSX2_Configurator.Core;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<GameModel> gameModels;

        private readonly SettingsService settingsService;
        private readonly GameLibraryService gameLibraryService;
        private readonly EmulationService emulationService;
        private readonly ConfigurationService configurationService;
        private readonly RemoteConfigurationService remoteConfigurationService;

        public MainWindow()
        {
            InitializeComponent();
            settingsService = new SettingsService(null);
            gameLibraryService = new GameLibraryService(null);
            emulationService = new EmulationService();
            configurationService = new ConfigurationService(settingsService.ConfigsDir);
            remoteConfigurationService = new RemoteConfigurationService(null);


            UpdateGameModels();
            gamesList.ItemsSource = gameModels; 
        }

        private void UpdateGameModels()
        {
            gameModels ??= new ObservableCollection<GameModel>();
            gameModels.Clear();
            foreach (var game in gameLibraryService.Games)
            {
                gameModels.Add(new GameModel
                {
                    Game = game.DisplayName ?? game.Name,
                    Path = game.Path,
                    Versions = settingsService.VersionsAndPaths.Keys,
                    Configs = settingsService.AvalialableConfigs.Keys,
                    Version = game.EmuVersion,
                    Config = game.Config
                });
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveBindings();
        }

        private void SaveBindings()
        {
            foreach (var game in gameModels)
            {
                var gameInfo = gameLibraryService.Games.FirstOrDefault(x => x.DisplayName == game.Game);
                if(gameInfo != null) gameLibraryService.UpdateGameInfo(gameInfo, new GameInfo(gameInfo) { EmuVersion = game.Version, Config = game.Config });
            }
        }

        private void ShowConfigWizard(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var configWizard = new ConfigWizard(configurationService, settingsService, model, UpdateGameModels);
            configWizard.Show();
        }


        private void DropIso(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (var file in files)
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var gameInfo = gameLibraryService.AddToLibrary(file);
                    var (name, region, id) = emulationService.IdentifyGame(settingsService.VersionsAndPaths["v1.4.0"], file);
                    gameLibraryService.UpdateGameInfo(gameInfo, new GameInfo(gameInfo) { DisplayName = name, Region = region, GameId = id }, shouldReloadLibrary: true);
                    Mouse.OverrideCursor = null;
                }
                UpdateGameModels();
            }
        }

        private void OpenSelection(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount >= 2)
            {
                var model = ((FrameworkElement)sender).GetBindingExpression(TextBlock.TextProperty).DataItem as GameModel;
                var emulatorPath = settingsService.VersionsAndPaths[model?.Version];
                var configPath = settingsService.AvalialableConfigs[model?.Config];
                EmulationService.LaunchWithGame(emulatorPath, model?.Path, configPath);
            }
        }
    }
}