using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using IniParser;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Settings;
using PCSX2_Configurator.Helpers;

namespace PCSX2_Configurator.Services
{
    internal sealed class EmulationService : IEmulationService
    {
        private readonly AppSettings appSettings;
        private readonly FileIniDataParser iniParser;
        private readonly IProcessHelpers processHelpers;
        private readonly IFileHelpers fileHelpers;

        public EmulationService(AppSettings appSettings, FileIniDataParser iniParser, IProcessHelpers processHelpers, IFileHelpers fileHelpers)
        {
            this.appSettings = appSettings;
            this.iniParser = iniParser;
            this.processHelpers = processHelpers;
            this.fileHelpers = fileHelpers;
        }

        public void LaunchWithGame(string emulatorPath, string gamePath, string configPath, string launchOptions)
        {
            EnsureUsingIso(configPath);
            var process = Process.Start(new ProcessStartInfo(emulatorPath, $"\"{gamePath}\" {launchOptions} --cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
            processHelpers.SendMessageCopyDataToWindowAnsi(appSettings.AutoHotkeyWindowHandle, $"GameIsRunning->{process.Id}");
        }

        public void LaunchWithConfig(string emulatorPath, string gamePath, string configPath)
        {
            UseIsoForGame(gamePath, configPath);
            Process.Start(new ProcessStartInfo(emulatorPath, $"--cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
        }

        public void ConfigureGraphicsPlugin(string emulatorPath, string configPath)
        {
            ConfigurePluginWithAutoHotkey("GS", emulatorPath, configPath);
        }

        public void ConfigureSoundPlugin(string emulatorPath, string configPath)
        {
            ConfigurePluginWithAutoHotkey("SPU2", emulatorPath, configPath);
        }

        public void ConfigureInputPlugin(string emulatorPath, string configPath)
        {
            ConfigurePluginWithAutoHotkey("PAD", emulatorPath, configPath);
        }

        public string GetInisPath(string emulatorPath)
        {
            var inisPath = $"{Path.GetDirectoryName(emulatorPath)}\\inis";
            inisPath = emulatorPath.Contains("1.4.0") ? inisPath.Replace("\\inis", "\\inis_1.4.0") : inisPath;
            return inisPath;
        }

        public void EnsureUsingIso(string inisPath)
        {
            var config = iniParser.ReadFile($"{inisPath}/{ConfiguratorConstants.UiFileName}");
            if (config.Global["CdvdSource"].ToLowerInvariant() != "iso")
            {
                config.Global["CdvdSource"] = "ISO";
                iniParser.WriteFile($"{inisPath}/{ConfiguratorConstants.UiFileName}", config, Encoding.UTF8);
            }
        }

        private void ConfigurePluginWithAutoHotkey(string plugin, string emulatorPath, string configPath)
        {
            var autoHotkeyWindowHandle = appSettings.AutoHotkeyWindowHandle;
            if (autoHotkeyWindowHandle != IntPtr.Zero)
            {
                var config = iniParser.ReadFile($"{configPath}/{ConfiguratorConstants.UiFileName}");
                var pluginsDir = config["Folders"]["PluginsFolder"];
                var pluginName = config["Filenames"][plugin];
                pluginsDir = Path.IsPathRooted(pluginsDir) ? pluginsDir : $"{Path.GetDirectoryName(emulatorPath)}\\{pluginsDir}";
                var messageData = $"Open{plugin}Plugin->{pluginsDir}\\{pluginName}|{configPath}".Replace(".dll", "");
                Task.Run(() => processHelpers.SendMessageCopyDataToWindowAnsi(autoHotkeyWindowHandle, messageData));
            }
        }

        private void UseIsoForGame(string gamePath, string configPath)
        {
            var config = iniParser.ReadFile($"{configPath}/{ConfiguratorConstants.UiFileName}");
            config.Global["CurrentIso"] = gamePath.Replace("\\","\\\\");
            iniParser.WriteFile($"{configPath}/{ConfiguratorConstants.UiFileName}", config, Encoding.UTF8);
        }
    }
}
