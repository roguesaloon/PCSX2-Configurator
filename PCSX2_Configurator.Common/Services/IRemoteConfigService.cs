using System.Collections.Generic;

namespace PCSX2_Configurator.Services
{
    public interface IRemoteConfigService
    {
        IEnumerable<string> AvailableConfigs { get; }
        void ImportConfig(string gameId, string emulatorPath);
        void ImportConfig(string configName, string emulatorPath, IEnumerable<string> gameIds);
    }
}