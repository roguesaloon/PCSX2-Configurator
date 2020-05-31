namespace PCSX2_Configurator.Services
{
    public interface IRemoteConfigService
    {
        void UpdateFromRemote();
        void ImportConfig(string directoryName, string inisPath);
    }
}
