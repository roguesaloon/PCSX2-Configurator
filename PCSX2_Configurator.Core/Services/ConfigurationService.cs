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
        public enum SettingsOptions
        {
            CopyLogSettings, CopyFolderSettings, CopyFileSettings, CopyWindowSettings,
            UseAdvancedSettings, 
            CopyVmSettings, CopyGsdxSettings, CopySpu2xSettings, CopyLilyPadSettings,
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

        public string CreateConfig(string configName, string inisPath, SettingsOptions settingsOptions)
        {
            var configPath = $"{configsDir}\\{configName}";
            Directory.CreateDirectory(configPath);

            CreateUiFile(configPath, inisPath, settingsOptions);

            if (settingsOptions.HasFlag(SettingsOptions.CopyVmSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{vmFileName}", $"{configPath}\\{vmFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopyGsdxSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{gsdxFileName}", $"{configPath}\\{gsdxFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopySpu2xSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{spu2xFileName}", $"{configPath}\\{spu2xFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopyLilyPadSettings)) FileHelpers.CopyWithoutException($"{inisPath}\\{lilyPadFileName}", $"{configPath}\\{lilyPadFileName}");

            return configPath;
        }

        public void ImportConfig(string configPath, string inisPath, SettingsOptions settingsOptions)
        {
            var configName = Path.GetDirectoryName(configPath);
            var importedConfigPath = CreateConfig(configName, inisPath, settingsOptions);

            foreach (var file in Directory.GetFiles(configPath))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, $"{importedConfigPath}\\{fileName}");
            }
        }

        private void CreateUiFile(string configPath, string inisPath, SettingsOptions settingsOptions)
        {
            var baseUiConfig = iniParser.ReadFile($"{inisPath}\\{uiFileName}");
            var targetUiConfig = new IniData();

            if (settingsOptions.HasFlag(SettingsOptions.CopyLogSettings)) targetUiConfig["ProgramLog"].Merge(baseUiConfig["ProgramLog"]);
            if (settingsOptions.HasFlag(SettingsOptions.CopyFolderSettings)) targetUiConfig["Folders"].Merge(baseUiConfig["Folders"]);
            if (settingsOptions.HasFlag(SettingsOptions.CopyFileSettings)) targetUiConfig["Filenames"].Merge(baseUiConfig["Filenames"]);
            if (settingsOptions.HasFlag(SettingsOptions.CopyWindowSettings)) targetUiConfig["GSWindow"].Merge(baseUiConfig["GSWindow"]);

            if (settingsOptions.HasFlag(SettingsOptions.UseAdvancedSettings))
            {
                targetUiConfig.Global["EnablePresets"] = "disabled";
                targetUiConfig.Global["EnableGameFixes"] = "enabled";
                targetUiConfig.Global["EnableSpeedHacks"] = "enabled";
            }

            iniParser.WriteFile($"{configPath}\\{uiFileName}", targetUiConfig, Encoding.UTF8);
        }
    }
}
