using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PCSX2_Configurator.Frontend.Wpf
{
    /// <summary>
    /// Interaction logic for LaunchOptions.xaml
    /// </summary>
    public partial class LaunchOptions : Window
    {
        public LaunchOptions()
        {
            InitializeComponent();
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