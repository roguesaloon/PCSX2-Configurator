using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using IniParser;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Settings;
using PCSX2_Configurator.Helpers;
using System.Runtime.InteropServices;

namespace PCSX2_Configurator.Services
{
    internal sealed class EmulationService : IEmulationService
    {
        private readonly AppSettings appSettings;
        private readonly FileIniDataParser iniParser;
        private readonly IProcessHelpers processHelpers;
        private readonly IFileHelpers fileHelpers;

        IProcessHelpers IEmulationService.ProcessHelpers => processHelpers;
        IFileHelpers IEmulationService.FileHelpers => fileHelpers;

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

        public async Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            var gsNullPluginOverride = $"--gs=\"{appSettings.AdditionalPluginsDirectory}\\GSnull.dll\"";
            var spu2NullPluginOverride = $"--spu2=\"{appSettings.AdditionalPluginsDirectory}\\SPU2null.dll\"";
            var cdvdNullPluginOverride = $"--cdvd=\"{appSettings.AdditionalPluginsDirectory}\\CDVDnull.dll\"";
            var padNullPluginOverride = $"--pad=\"{appSettings.AdditionalPluginsDirectory}\\xpad.dll\"";
            var usbNullPluginOverride = $"--usb=\"{appSettings.AdditionalPluginsDirectory}\\USBnull.dll\"";
            var dev9NullPluginOverride = $"--dev9=\"{appSettings.AdditionalPluginsDirectory}\\DEV9null.dll\"";
            var fwNullPluginOverride = $"--fw=\"{appSettings.AdditionalPluginsDirectory}\\FWnull.dll\"";
            /*var startInfo = new ProcessStartInfo(emulatorPath, 
                $"\"{gamePath}\" --windowed --nogui --console " +
                $"{gsNullPluginOverride} {spu2NullPluginOverride} {cdvdNullPluginOverride} {padNullPluginOverride} {usbNullPluginOverride} {dev9NullPluginOverride} {fwNullPluginOverride}")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            };*/ // Is This the same as below? Is native method needed here?

            var emulatorProcessId = processHelpers.StartProcess(emulatorPath,
                $"\"{gamePath}\" --windowed --nogui --console " +
                $"{gsNullPluginOverride} {spu2NullPluginOverride} {cdvdNullPluginOverride} {padNullPluginOverride} {usbNullPluginOverride} {dev9NullPluginOverride} {fwNullPluginOverride}",
                Path.GetDirectoryName(emulatorPath), 0);

            using var emulator = Process.GetProcessById(emulatorProcessId);
            var gameInfo = await ReadInfoFromEmulatorWindow(emulator);
            if(emulator.HasExited) return await IdentifyGame(emulatorPath, gamePath);

            emulator.Kill();
            emulator.WaitForExit();
            emulator.Close();
            return gameInfo.gameId != null ? gameInfo : await IdentifyGame(emulatorPath, gamePath);
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

        private async Task<(string gameTitle, string gameRegion, string gameId)> ReadInfoFromEmulatorWindow(Process runningEmulator, int retryCount = 0)
        {
            var gameTitle = default(string);
            var gameRegion = default(string);
            var gameId = default(string);
            await Task.Delay(2000);
            var window = processHelpers.FindWindowForProcess(runningEmulator.Id, title => title.ToLowerInvariant() != "pcsx2");
            if (window.title == null || string.IsNullOrEmpty(window.title)) return (gameTitle, gameRegion, gameId);
            if ((window.title.ToLowerInvariant().Contains("pcsx2") || window.title.Contains("BIOS")) && retryCount < 10) return await ReadInfoFromEmulatorWindow(runningEmulator, ++retryCount);
            var idMatch = Regex.Match(window.title, "\\[[A-Z]{4}-[0-9]{5}]");
            if (idMatch.Success)
            {
                var regionMatch = Regex.Match(window.title, "\\(.*?\\)");
                gameTitle = window.title.Substring(0, regionMatch.Index).Trim();
                gameRegion = regionMatch?.Value[1..^1];
                gameId = idMatch?.Value?.Substring(1, 10);
            }
            else gameId = "???";

            return (gameTitle, gameRegion, gameId);
        }
    }
}
