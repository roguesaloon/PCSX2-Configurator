using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PCSX2_Configurator.Core;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Frontend.Wpf
{
    public partial class App : Application
    {
        private readonly IHost host;

        static App()
        {
            LoadAssembliesFromDirectory($"{Directory.GetCurrentDirectory()}\\ThirdParty");
            LoadAssembliesFromDirectory($"{Directory.GetCurrentDirectory()}\\Microsoft");
        }

        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) => builder.AddJsonFile("Settings.json", optional: false))
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var settings = new AppSettings();
            context.Configuration.Bind(settings, options => options.BindNonPublicProperties = true);
            services.AddTransient(provider => settings);

            services.AddSingleton(provider => new GameLibraryService(settings.GameLibraryFile));
            services.AddSingleton(provider => new EmulationService());
            services.AddSingleton(provider => new ConfigurationService(settings.ConfigsDirectory));
            services.AddSingleton<ICoverService>(provider => new ChainedCoverService(settings.Covers.CoversPath, settings.Covers.MissingCover));
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            host.Services.GetRequiredService<MainWindow>().Show();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            host.Dispose();
            base.OnExit(e);
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
