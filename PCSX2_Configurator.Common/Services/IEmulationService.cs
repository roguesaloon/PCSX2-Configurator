namespace PCSX2_Configurator.Services
{
    public interface IEmulationService
    {
        void LaunchWithGame(string emulatorPath, string gamePath, string configPath, string launchOptions);
        void LaunchWithConfig(string emulatorPath, string gamePath, string configPath);
        void ConfigureGraphicsPlugin(string emulatorPath, string configPath);
        void ConfigureSoundPlugin(string emulatorPath, string configPath);
        void ConfigureInputPlugin(string emulatorPath, string configPath);
        string GetInisPath(string emulatorPath);
        void EnsureUsingIso(string inisPath);
    }
}
