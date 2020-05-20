﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IniParser;
using SevenZip;
using PCSX2_Configurator.Helpers;
using PCSX2_Configurator.Services;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Extensions.DependencyInjection
{
    public static class Pcsx2ConfiguratorServiceConfigurationExtensions
    {
        public static void AddPcsx2ConfiguratorCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new AppSettings();
            configuration.Bind(settings, options => options.BindNonPublicProperties = true);
            services.AddTransient(provider => settings);

            services.AddTransient<IProcessHelpers, WindowsProcessHelpers>();
            
            SevenZipBase.SetLibraryPath(settings.SevenZipLibraryPath);
            services.AddTransient<FileIniDataParser>();

            services.AddSingleton<IGameLibraryService, GameLibraryService>();
            services.AddSingleton<IEmulationService, EmulationService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ICoverService, ChainedCoverService>();
            services.AddSingleton<IVersionManagementService, VersionManagementService>();
        }
    }
}