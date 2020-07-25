using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCSX2_Configurator.Extensions.DependencyInjection;
using PCSX2_Configurator.Frontend.Wpf.Windows;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public partial class App : Application
    {
        private readonly IHost host;
        private Process autoHotkeyProcess;
        private static IServiceProvider ServiceProvider { get; set; }
        public static T Get<T>() => ServiceProvider.GetRequiredService<T>();

        static App()
        {
            LoadAssembliesFromDirectory($"{Directory.GetCurrentDirectory()}\\ThirdParty");
        }

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) => builder.AddJsonFile("Settings.json", optional: false, reloadOnChange: true))
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHttpClient();
            var settings = services.AddPcsx2ConfiguratorCoreServices(context.Configuration);

            typeof(ConfigModel).GetProperty(nameof(ConfigModel.ConfigsDirectory)).SetValue(null, settings.ConfigsDirectory);

            services.AddSingleton<MainWindow>();
            services.AddTransient<ConfigWizard>();
            services.AddTransient<VersionManager>();
            services.AddTransient<RemoteConfigImporter>();
            services.AddTransient<LaunchOptions>();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ServiceProvider = host.Services;
            Get<MainWindow>().Show();
            StartAutoHotkeyScript();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            host.Dispose();
            autoHotkeyProcess?.Kill();
        }

        private void StartAutoHotkeyScript()
        {
            var settings = Get<AppSettings>();
            var executable = settings.AutoHotkeyExecutable;
            var scriptPath = settings.AutoHotkeyScript;
            if (File.Exists(executable) && File.Exists(scriptPath))
            {
                autoHotkeyProcess = Process.Start(executable, $"\"{scriptPath}\"");
                Task.Delay(500)
                    .ContinueWith(x => typeof(AppSettings).GetProperty(nameof(settings.AutoHotkeyWindowHandle))
                    .SetValue(settings, autoHotkeyProcess.MainWindowHandle));
            }
        }

        private static void LoadAssembliesFromDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var assembly in Directory.GetFiles(path, "*dll"))
                {
                    Assembly.LoadFrom(assembly);
                }
            }
        }
    }
}
