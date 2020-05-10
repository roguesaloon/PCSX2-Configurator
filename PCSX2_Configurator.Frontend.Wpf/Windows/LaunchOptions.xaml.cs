using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for LaunchOptions.xaml
    /// </summary>
    public partial class LaunchOptions : Window
    {
        private GameModel gameModel;

        public LaunchOptions()
        {
            InitializeComponent();
        }

        public void Show(GameModel gameModel)
        {
            this.gameModel = gameModel;
            @params.Text = gameModel.LaunchOptions;
            Show();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            gameModel.LaunchOptions = @params.Text;
        }

        private void AddFullScreenParam(object sender, MouseButtonEventArgs e) => SetParam("--fullscreen");

        private void AddNoGuiParam(object sender, MouseButtonEventArgs e) => SetParam("--nogui");

        private void AddFullBootParam(object sender, MouseButtonEventArgs e) => SetParam("--fullboot");

        private void SetParam(string param)
        {
            @params.Text += (string.IsNullOrWhiteSpace(@params.Text) ? "" : " ") + param;
            @params.CaretIndex = @params.Text.Length;
        }
    }
}