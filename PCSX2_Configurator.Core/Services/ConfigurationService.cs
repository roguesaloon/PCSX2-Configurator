using IniParser;
using IniParser.Model;
using System;
using System.IO;
using System.Text;

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
        readonly FileIniDataParser iniParser;
        public ConfigurationService(string configsDir)
        {
            iniParser = new FileIniDataParser();
            this.configsDir = configsDir;
        }

        public void CreateConfig(string configName, string inisPath, SettingsOptions settingsOptions)
        {
            var configPath = $"{configsDir}\\{configName}";
            Directory.CreateDirectory(configPath);

            CreateUiFile(configPath, inisPath, settingsOptions);

            if (settingsOptions.HasFlag(SettingsOptions.CopyVmSettings)) File.Copy($"{inisPath}\\{vmFileName}", $"{configPath}\\{vmFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopyGsdxSettings)) File.Copy($"{inisPath}\\{gsdxFileName}", $"{configPath}\\{gsdxFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopySpu2xSettings)) File.Copy($"{inisPath}\\{spu2xFileName}", $"{configPath}\\{spu2xFileName}");
            if (settingsOptions.HasFlag(SettingsOptions.CopyLilyPadSettings)) File.Copy($"{inisPath}\\{lilyPadFileName}", $"{configPath}\\{lilyPadFileName}");
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
