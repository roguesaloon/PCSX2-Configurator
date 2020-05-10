using System;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class ConfigurationService
    {
        [Flags]
        public enum ConfigOptions
        {
            None = 0,
            CopyLogSettings = 1, CopyFolderSettings = 2, CopyFileSettings = 4, CopyWindowSettings = 8,
            DisablePresets = 16, EnableGameFixes = 32, EnableSpeedHacks = 64,
            CopyVmSettings = 128, CopyGsdxSettings = 256, CopySpu2xSettings = 512, CopyLilyPadSettings = 1024,
            UseAdvancedSettings = DisablePresets | EnableGameFixes | EnableSpeedHacks,
            All = CopyLogSettings | CopyFolderSettings | CopyFileSettings | CopyWindowSettings | UseAdvancedSettings | CopyVmSettings | CopyGsdxSettings | CopySpu2xSettings | CopyLilyPadSettings
        }

        private const string uiFileName = "PCSX2_ui.ini";
        private const string vmFileName = "PCSX2_vm.ini";
        private const string gsdxFileName = "Gsdx.ini";
        private const string spu2xFileName = "SPU2-X.ini";
        private const string lilyPadFileName = "LilyPad.ini";

        private readonly string configsDir;
        private readonly FileIniDataParser iniParser;
        public ConfigurationService(AppSettings appSettings)
        {
            iniParser = new FileIniDataParser();
            configsDir = appSettings.ConfigsDirectory;
        }

        public string CreateConfig(string configName, string inisPath, ConfigOptions configOptions)
        {
            var configPath = $"{configsDir}\\{configName}";
            Directory.CreateDirectory(configPath);

            CreateUiFile(configPath, inisPath, configOptions);

            if (configOptions.HasFlag(ConfigOptions.CopyVmSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{vmFileName}", $"{configPath}\\{vmFileName}");
            if (configOptions.HasFlag(ConfigOptions.CopyGsdxSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{gsdxFileName}", $"{configPath}\\{gsdxFileName}");
            if (configOptions.HasFlag(ConfigOptions.CopySpu2xSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{spu2xFileName}", $"{configPath}\\{spu2xFileName}");
            if (configOptions.HasFlag(ConfigOptions.CopyLilyPadSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{lilyPadFileName}", $"{configPath}\\{lilyPadFileName}");

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

        private void CreateUiFile(string configPath, string inisPath, ConfigOptions settingsOptions)
        {
            var baseUiConfig = iniParser.ReadFile($"{inisPath}\\{uiFileName}");
            var targetUiConfig = new IniData();

            if (settingsOptions.HasFlag(ConfigOptions.CopyLogSettings)) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (settingsOptions.HasFlag(ConfigOptions.CopyFolderSettings)) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (settingsOptions.HasFlag(ConfigOptions.CopyFileSettings)) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (settingsOptions.HasFlag(ConfigOptions.CopyWindowSettings)) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (settingsOptions.HasFlag(ConfigOptions.DisablePresets)) targetUiConfig.Global["EnablePresets"] = "disabled";
            if (settingsOptions.HasFlag(ConfigOptions.EnableGameFixes)) targetUiConfig.Global["EnableGameFixes"] = "enabled";
            if (settingsOptions.HasFlag(ConfigOptions.EnableSpeedHacks)) targetUiConfig.Global["EnableSpeedHacks"] = "enabled";

            iniParser.WriteFile($"{configPath}\\{uiFileName}", targetUiConfig, Encoding.UTF8);
        }
    }
}
