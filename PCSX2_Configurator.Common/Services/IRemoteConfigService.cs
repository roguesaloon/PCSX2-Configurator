namespace PCSX2_Configurator.Services
{
    public interface IRemoteConfigService
    {
        void ImportConfig(string gameId, string emulatorPath);
    }
}
