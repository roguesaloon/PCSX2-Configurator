
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Helpers;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Services
{
    internal sealed class EmulatorIdentificationService : BaseIdentificationService
    {
        private readonly AppSettings appSettings;
        private readonly IEmulationService emulationService;
        private readonly IFileHelpers fileHelpers;
        private readonly IProcessHelpers processHelpers;

        public EmulatorIdentificationService(AppSettings appSettings, IGameLibraryService gameLibraryService, ICoverService coverService, IEmulationService emulationService, IFileHelpers fileHelpers, IProcessHelpers processHelpers) : base(gameLibraryService, coverService)
        {
            this.appSettings = appSettings;
            this.emulationService = emulationService;
            this.fileHelpers = fileHelpers;
            this.processHelpers = processHelpers;
        }

        public async override Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
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
            if (emulator.HasExited) return await IdentifyGame(emulatorPath, gamePath);

            emulator.Kill();
            emulator.WaitForExit();
            emulator.Close();
            return gameInfo.gameId != null ? gameInfo : await IdentifyGame(emulatorPath, gamePath);
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

        public async override Task ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, Action<GameInfo, string> callback)
        {
            var inisPath = emulationService.GetInisPath(emulatorPath);
            emulationService.EnsureUsingIso(inisPath);
            fileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", true);

            await base.ImportGames(emulatorPath, gameInfos, callback);

            fileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", false);
        }
    }
}
