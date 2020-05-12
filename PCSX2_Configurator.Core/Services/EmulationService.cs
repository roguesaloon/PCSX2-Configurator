using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using IniParser;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class EmulationService
    {
        private AppSettings appSettings;
        private readonly FileIniDataParser iniParser;
        private readonly IProcessHelpers processHelpers;

        public EmulationService(AppSettings appSettings)
        {
            this.appSettings = appSettings;
            iniParser = new FileIniDataParser();
            processHelpers = new WindowsProcessHelpers();
        }

        public void LaunchWithGame(string emulatorPath, string gamePath, string configPath, string launchOptions)
        {
            configPath = Path.GetFullPath(configPath);
            EnsureUsingIso(configPath);
            Process.Start(new ProcessStartInfo(emulatorPath, $"\"{gamePath}\" {launchOptions} --cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
        }

        public static void LaunchWithConfig(string emulatorPath, string configPath)
        {
            configPath = Path.GetFullPath(configPath);
            Process.Start(new ProcessStartInfo(emulatorPath, $"--cfgpath=\"{configPath}\"")
            {
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            });
        }

        public void ConfigureGraphicsPlugin(string emulatorPath, string configPath)
        {
            var autoHotkeyWindowHandle = appSettings.AutoHotkeyWindowHandle;
            if(autoHotkeyWindowHandle != null)
            {
                processHelpers.SendMessageCopyDataToWindowAnsi(autoHotkeyWindowHandle, "OpenGSPlugin_{pluginPath}");
            }
        }

        public static string GetInisPath(string emulatorPath)
        {
            var inisPath = $"{Path.GetDirectoryName(emulatorPath)}\\inis";
            inisPath = emulatorPath.Contains("1.4.0") ? inisPath.Replace("\\inis", "\\inis_1.4.0") : inisPath;
            return inisPath;
        }

        public (string gameTitle, string gameRegion, string gameId) IdentifyGame(string emulatorPath, string gamePath)
        {
            var inisPath = GetInisPath(emulatorPath);
            var additionalPluginsDirectory = Path.GetFullPath(appSettings.AdditionalPluginsDirectory);
            EnsureUsingIso(inisPath);
            var startInfo = new ProcessStartInfo(emulatorPath, $"\"{gamePath}\" --windowed --nogui --console --gs=\"{additionalPluginsDirectory}\\GSnull.dll\"")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized,
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)
            };

            using var emulator = Process.Start(startInfo);
            var gameInfo = ReadInfoFromEmulatorWindow(emulator, retryCount: 4);
            emulator.Kill();

            return gameInfo;
        }

        private void EnsureUsingIso(string inisPath)
        {
            var config = iniParser.ReadFile($"{inisPath}/PCSX2_ui.ini");
            config.Global["CdvdSource"] = "ISO";
            iniParser.WriteFile($"{inisPath}/PCSX2_ui.ini", config);
        }

        private (string gameTitle, string gameRegion, string gameId) ReadInfoFromEmulatorWindow(Process runningEmulator, int retryCount)
        {
            var gameTitle = default(string);
            var gameRegion = default(string);
            var gameId = default(string);
            for (int i = 0; i < retryCount; ++i)
            {
                Thread.Sleep(2000);
                var windowTitle = processHelpers.GetWindowTitleText(runningEmulator.MainWindowHandle);
                var idMatch = Regex.Match(windowTitle, "\\[[A-Z]{4}-[0-9]{5}]");
                if (idMatch.Success)
                {
                    var regionMatch = Regex.Match(windowTitle, "\\(.*?\\)");
                    gameTitle = windowTitle.Substring(0, regionMatch.Index).Trim();
                    gameRegion = regionMatch.Value.Substring(1, regionMatch.Value.Length - 2);
                    gameId = idMatch.Value.Substring(1, 10);
                    break;
                }
            }

            return (gameTitle, gameRegion, gameId);
        }
    }
}
