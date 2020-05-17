using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;
using SevenZip;
using PCSX2_Configurator.Settings;
using static PCSX2_Configurator.Core.ConfigOptions;

namespace PCSX2_Configurator.Core
{
    public sealed class ConfigurationService
    {
        private const string uiFileName = "PCSX2_ui.ini";
        private const string vmFileName = "PCSX2_vm.ini";
        private const string gsdxFileName = "Gsdx.ini";
        private const string spu2xFileName = "SPU2-X.ini";
        private const string lilyPadFileName = "LilyPad.ini";

        private readonly string configsDir;
        private readonly FileIniDataParser iniParser;
        private readonly string compressedMemoryCard;

        public ConfigurationService(AppSettings appSettings)
        {
            iniParser = new FileIniDataParser();
            configsDir = appSettings.ConfigsDirectory;
            compressedMemoryCard = appSettings.CompressedMemCard;
            SevenZipBase.SetLibraryPath(appSettings.SevenZipLibraryPath);
        }

        public string CreateConfig(string configName, string inisPath, ConfigOptions configOptions)
        {
            var configPath = $"{configsDir}\\{configName}";
            Directory.CreateDirectory(configPath);

            CreateUiFile(configPath, inisPath, configOptions);

            if (configOptions.Flags.HasFlag(ConfigFlags.CopyVmSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{vmFileName}", $"{configPath}\\{vmFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyGsdxSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{gsdxFileName}", $"{configPath}\\{gsdxFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopySpu2xSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{spu2xFileName}", $"{configPath}\\{spu2xFileName}");
            if (configOptions.Flags.HasFlag(ConfigFlags.CopyLilyPadSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{lilyPadFileName}", $"{configPath}\\{lilyPadFileName}");

            SetVmSettings(configPath, configOptions);

            return configPath;
        }

        public void ImportConfig(string configPath, string inisPath, ConfigOptions settingsOptions)
        {
            var configName = Path.GetDirectoryName(configPath);
            var importedConfigPath = CreateConfig(configName, inisPath, settingsOptions);

            foreach (var file in Directory.GetFiles(configPath))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, $"{importedConfigPath}\\{fileName}");
            }
        }

        private void SetVmSettings(string configPath, ConfigOptions configOptions)
        {
            var targetVmFile = $"{configPath}\\{vmFileName}";
            var targetVmConfig = File.Exists(targetVmFile) ? iniParser.ReadFile(targetVmFile) : new IniData();

            if (configOptions.Flags.HasFlag(ConfigFlags.EnableWidescreenPatches)) targetVmConfig["EmuCore"]["EnableWideScreenPatches"] = "enabled";
            if (configOptions.Flags.HasFlag(ConfigFlags.EnableCheats)) targetVmConfig["EmuCore"]["EnableCheats"] = "enabled";

            iniParser.WriteFile(targetVmFile, targetVmConfig, Encoding.UTF8);
        }

        private void CreateUiFile(string configPath, string inisPath, ConfigOptions configOptions)
        {
            var baseUiConfig = iniParser.ReadFile($"{inisPath}\\{uiFileName}");
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

            iniParser.WriteFile($"{configPath}\\{uiFileName}", targetUiConfig, Encoding.UTF8);
        }
    }
}
