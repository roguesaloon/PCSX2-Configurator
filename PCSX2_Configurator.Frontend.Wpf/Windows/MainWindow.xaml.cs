
using System;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PCSX2_Configurator.Core;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<GameModel> gameModels;

        private readonly AppSettings settings;
        private readonly GameLibraryService gameLibraryService;
        private readonly EmulationService emulationService;
        private readonly ICoverService coverService;

        private void CloseWindow(object sender, RoutedEventArgs e) => Close();
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;  
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void FullscreenWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            titleBar_Maximize.Content = WindowState == WindowState.Maximized ? "🗗": "🗖";
            titleBar_Fullscreen.Visibility = ResizeMode == ResizeMode.CanResize || WindowState != WindowState.Maximized ? Visibility.Visible : Visibility.Hidden;
            BorderThickness = new Thickness(WindowState == WindowState.Maximized ? 8 : 0);

            var windowChrome = WindowChrome.GetWindowChrome(this);
            windowChrome.GlassFrameThickness = new Thickness(WindowState == WindowState.Maximized ? 1 : 0);
            WindowChrome.SetWindowChrome(this, windowChrome);

            base.OnStateChanged(e);
        }

        public MainWindow(AppSettings settings, GameLibraryService gameLibraryService, EmulationService emulationService, ICoverService coverService)
        {
            InitializeComponent();
            this.settings = settings;
            this.gameLibraryService = gameLibraryService;
            this.emulationService = emulationService;
            this.coverService = coverService;

            PopulateGameModelsFromLibrary();
            gamesList.ItemsSource = gameModels; 
        }

        private void PopulateGameModelsFromLibrary()
        {
            GameModel.Versions = settings.Versions?.Keys;
            GameModel.Configs = settings.Configs?.Keys;
            gameModels = new ObservableCollection<GameModel>();

            gameLibraryService.Games.ForEach(game => AddGameModel(game));
            GetAllCoversAsync();
        }

        private void GetAllCoversAsync()
        {
            Task.Run(() => Parallel.For(0, gameModels.Count, async index =>
                gameModels[index].CoverPath = await coverService.GetCoverForGame(gameLibraryService.Games[index])));
        }

        private void AddGameModel(GameInfo game)
        {
            gameModels.Add(new GameModel
            {
                Game = game.DisplayName ?? game.Name,
                Path = game.Path,
                Version = game.EmuVersion,
                Config = game.Config,
                LaunchOptions = game.LaunchOptions,
                CoverPath = settings.Covers.LoadingCover,
                GameInfo = game
            });
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            SaveBindings();
        }

        private void SaveBindings()
        {
            foreach (var game in gameModels)
            {
                var gameInfo = gameLibraryService.Games.FirstOrDefault(x => x.DisplayName == game.Game);
                if(gameInfo != null) gameLibraryService.UpdateGameInfo(gameInfo, new GameInfo(gameInfo) { EmuVersion = game.Version, Config = game.Config, LaunchOptions = game.LaunchOptions });
            }
        }

        private void ShowConfigWizard(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            App.Get<ConfigWizard>().Show(model);
        }


        private void DropIso(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            files = files
                .SelectMany(path => Directory.Exists(path) ? FileHelpers.GetFilesToDepth(path, 3) : new string[] { path })
                .Where(file => new string[] { ".iso", ".mdf", ".nrg", ".bin", ".img", ".dump", ".gz", ".cso" }.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .OrderBy(file => file)
                .ToArray();

            var versionToUse = VersionManagementService.GetMostRecentStableVersion(settings.Versions.Keys);
            if (versionToUse == null)
            {
                MessageBox.Show("PCSX2 Configurator requires at least one installed PCSX2 version", "Error");
                return;
            }

            foreach (var file in files)
            {
                var gameInfo = gameLibraryService.AddToLibrary(file);
                AddGameModel(gameInfo);
            }

            Task.Run(() =>
            {
                foreach (var model in gameModels)
                {
                    if (model.GameInfo.GameId != null) continue;
                    var (name, region, id) = emulationService.IdentifyGame(settings.Versions[versionToUse], model.Path);
                    gameLibraryService.UpdateGameInfo(model.GameInfo, new GameInfo(model.GameInfo) { DisplayName = name, Region = region, GameId = id }, shouldReloadLibrary: true);
                    Task.Run(async () => model.CoverPath = await coverService.GetCoverForGame(model.GameInfo));
                }
            });
        }

        private void SetVersion(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var model = ((Control) menuItem.GetBindingExpression(BindingGroupProperty).DataItem).DataContext as GameModel;
            model.Version = menuItem.Header as string;
        }

        private void SetConfig(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var model = ((Control)menuItem.GetBindingExpression(BindingGroupProperty).DataItem).DataContext as GameModel;
            model.Config = menuItem.Header as string;
        }

        private void RemoveGame(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var gameInfo = gameLibraryService.Games.FirstOrDefault(x => x.DisplayName == model.Game || x.Name == model.Game);
            gameLibraryService.RemoveFromLibrary(gameInfo);
            gameModels.Remove(model);
        }

        private void ConfigGame(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var version = model?.Version ?? string.Empty;
            var config = model?.Config ?? string.Empty;
            if (!settings.Versions.ContainsKey(version) || !settings.Configs.ContainsKey(config))
            {
                MessageBox.Show("This Game is not configured", "Error");
                return;
            }
            var emulatorPath = settings.Versions[version];
            var configPath = settings.Configs[config];
            emulationService.LaunchWithConfig(emulatorPath, model?.Path, configPath);
        }

        private void ConfigureGraphicsPlugin(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var version = model?.Version ?? string.Empty;
            var config = model?.Config ?? string.Empty;
            var emulatorPath = settings.Versions[version];
            var configPath = settings.Configs[config];

            emulationService.ConfigureGraphicsPlugin(emulatorPath, configPath);
        }

        private void ConfigureSoundPlugin(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var version = model?.Version ?? string.Empty;
            var config = model?.Config ?? string.Empty;
            var emulatorPath = settings.Versions[version];
            var configPath = settings.Configs[config];

            emulationService.ConfigureSoundPlugin(emulatorPath, configPath);
        }

        private void ConfigureInputPlugin(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            var version = model?.Version ?? string.Empty;
            var config = model?.Config ?? string.Empty;
            var emulatorPath = settings.Versions[version];
            var configPath = settings.Configs[config];

            emulationService.ConfigureInputPlugin(emulatorPath, configPath);
        }

        private void SetLaunchOptions(object sender, RoutedEventArgs e)
        {
            var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
            App.Get<LaunchOptions>().Show(model);
        }

        private void OpenVersionManager(object sender, RoutedEventArgs e)
        {
            App.Get<VersionManager>().Show();
        }

        private void StartGame(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount >= 2)
            {
                var model = ((FrameworkElement)sender).GetBindingExpression(BindingGroupProperty).DataItem as GameModel;
                var version = model?.Version ?? string.Empty;
                var config = model?.Config ?? string.Empty;
                if (!settings.Versions.ContainsKey(version) || !settings.Configs.ContainsKey(config))
                {
                    MessageBox.Show("This Game is not configured", "Error");
                    return;
                }
                var emulatorPath = settings.Versions[version];
                var configPath = settings.Configs[config];
                emulationService.LaunchWithGame(emulatorPath, model?.Path, configPath, model?.LaunchOptions);
            }
        }
    }
}