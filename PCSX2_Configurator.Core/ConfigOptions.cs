using System;

namespace PCSX2_Configurator.Core
{
    public class ConfigOptions
    {
        [Flags]
        public enum ConfigFlags
        {
            None = 0,
            CopyLogSettings = 1, CopyFolderSettings = 2, CopyFileSettings = 4, CopyWindowSettings = 8,
            CopyVmSettings = 16, CopyGsdxSettings = 32, CopySpu2xSettings = 64, CopyLilyPadSettings = 128,
            EnableWidescreenPatches = 256, EnableCheats = 512, CreateMemoryCard = 1024,
            DisablePresets = 2048, EnableGameFixes = 4096, EnableSpeedHacks = 8192,
            UseAdvancedSettings = DisablePresets | EnableGameFixes | EnableSpeedHacks,
            All = CopyLogSettings | CopyFolderSettings | CopyFileSettings | CopyWindowSettings | UseAdvancedSettings | CopyVmSettings | CopyGsdxSettings | CopySpu2xSettings | CopyLilyPadSettings
        }

        public enum ConfigAspectRatio
        {
            Original, Widescreen, Stretched
        }

        public ConfigFlags Flags { get; set; }
        public ConfigAspectRatio AspectRatio { get; set; }
        public int? ZoomLevel { get; set; }

        public static ConfigOptions Default => new ConfigOptions
        {
            Flags = ConfigFlags.All,
            AspectRatio = ConfigAspectRatio.Original,
            ZoomLevel = 100
        };
    }
}
