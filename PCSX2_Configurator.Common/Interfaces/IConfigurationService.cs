using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Services
{
    public interface IConfigurationService
    {
        string CreateConfig(string configName, string inisPath, ConfigOptions configOptions);
        void ImportConfig(string configPath, string inisPath, ConfigOptions settingsOptions);
    }
}
