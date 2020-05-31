using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;
using SevenZip;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Helpers;
using PCSX2_Configurator.Settings;
using static PCSX2_Configurator.Common.ConfigOptions;

namespace PCSX2_Configurator.Services
{
    internal sealed class ConfigurationService : IConfigurationService
    {
        private readonly FileIniDataParser iniParser;
        private readonly IFileHelpers fileHelpers;
        private readonly string configsDir;
        private readonly string compressedMemoryCard;

        public ConfigurationService(AppSettings appSettings, FileIniDataParser iniParser, IFileHelpers fileHelpers)
        {
            this.iniParser = iniParser;
            this.fileHelpers = fileHelpers;
            configsDir = appSettings.ConfigsDirectory;
            compressedMemoryCard = appSettings.CompressedMemCard;
        }

        public string CreateConfig(string configName, string inisPath, ConfigOptions configOptions)
        {
            var configPath = $"{configsDir}\\{configName}";
            Directory.CreateDirectory(configPath);

            CreateUiFile(configPath, inisPath, configOptions);

            if (configOptions.Flags.HasFlag(ConfigFlags.CopyVmSettings)) fileHelpers.CopyWithoutException($"{inisPath}\\{ConfiguratorConstants.VmFileName}", $"{configPath}\\{ConfiguratorConstants.VmFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyGsdxSettings)) fileHelpers.CopyWithoutException($"{inisPath}\\{ConfiguratorConstants.GsdxFileName}", $"{configPath}\\{ConfiguratorConstants.GsdxFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopySpu2xSettings)) fileHelpers.CopyWithoutException($"{inisPath}\\{ConfiguratorConstants.Spu2xFileName}", $"{configPath}\\{ConfiguratorConstants.Spu2xFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyLilyPadSettings)) fileHelpers.CopyWithoutException($"{inisPath}\\{ConfiguratorConstants.LilyPadFileName}", $"{configPath}\\{ConfiguratorConstants.LilyPadFileName}");

            SetVmSettings(configPath, configOptions);

            return configPath;
        }

        private void SetVmSettings(string configPath, ConfigOptions configOptions)
        {
            var targetVmFile = $"{configPath}\\{ConfiguratorConstants.VmFileName}";
            var targetVmConfig = File.Exists(targetVmFile) ? iniParser.ReadFile(targetVmFile) : new IniData();

            if (configOptions.Flags.HasFlag(ConfigFlags.EnableWidescreenPatches)) targetVmConfig["EmuCore"]["EnableWideScreenPatches"] = "enabled";
            if (configOptions.Flags.HasFlag(ConfigFlags.EnableCheats)) targetVmConfig["EmuCore"]["EnableCheats"] = "enabled";

            iniParser.WriteFile(targetVmFile, targetVmConfig, Encoding.UTF8);
        }

        private void CreateUiFile(string configPath, string inisPath, ConfigOptions configOptions)
        {
            var baseUiConfig = iniParser.ReadFile($"{inisPath}\\{ConfiguratorConstants.UiFileName}");
            var targetUiConfig = new IniData();

            if (configOptions.Flags.HasFlag(ConfigFlags.CopyLogSettings)) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyFolderSettings)) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyFileSettings)) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyWindowSettings)) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (configOptions.Flags.HasFlag(ConfigFlags.DisablePresets)) targetUiConfig.Global["EnablePresets"] = "disabled";
            if (configOptions.Flags.HasFlag(ConfigFlags.EnableGameFixes)) targetUiConfig.Global["EnableGameFixes"] = "enabled";
            if (configOptions.Flags.HasFlag(ConfigFlags.EnableSpeedHacks)) targetUiConfig.Global["EnableSpeedHacks"] = "enabled";

            if (configOptions.ZoomLevel != null) targetUiConfig["GSWindow"]["Zoom"] = configOptions.ZoomLevel.ToString();
            targetUiConfig["GSWindow"]["AspectRatio"] =
                configOptions.AspectRatio == ConfigAspectRatio.Original ? "4:3":
                configOptions.AspectRatio == ConfigAspectRatio.Widescreen ? "16:9":
                configOptions.AspectRatio == ConfigAspectRatio.Stretched ? "Stretch": "";

            if (configOptions.Flags.HasFlag(ConfigFlags.CreateMemoryCard))
            {
                targetUiConfig["Folders"]["UseDefaultMemoryCards"] = "disabled";
                targetUiConfig["Folders"]["MemoryCards"] = configPath.Replace("\\","\\\\");
                targetUiConfig["MemoryCards"]["Slot1_Enable"] = "enabled";
                targetUiConfig["MemoryCards"]["Slot2_Enable"] = "disabled";
                targetUiConfig["MemoryCards"]["Slot1_Filename"] = "Mcd.ps2";
                targetUiConfig["MemoryCards"]["Slot2_Filename"] = string.Empty;
                using var extractor = new SevenZipExtractor(compressedMemoryCard);
                extractor.ExtractArchive(configPath);
            } 

            iniParser.WriteFile($"{configPath}\\{ConfiguratorConstants.UiFileName}", targetUiConfig, Encoding.UTF8);
        }
    }
}
