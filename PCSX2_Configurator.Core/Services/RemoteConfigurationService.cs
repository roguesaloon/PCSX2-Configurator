using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class RemoteConfigurationService
    {
        private readonly string remoteConfigsPath;
        private readonly string remote = "https://github.com/Zombeaver/PCSX2-Configs";
        private readonly ConfigurationService configurationService;

        public RemoteConfigurationService(ConfigurationService configurationService, AppSettings appSettings)
        {
            remoteConfigsPath = appSettings.RemoteConfigsPath ?? "Remote";
            this.configurationService = configurationService;

            UpdateFromRemote();
        }

        public void ImportConfig(string directoryName, string inisPath)
        {
            if (!Directory.GetDirectories(remoteConfigsPath).Any(directory => directory == directoryName)) throw new Exception("Config does not exist");
            configurationService.ImportConfig($"{remoteConfigsPath}\\{directoryName}", inisPath, ConfigurationService.SettingsOptions.All);
        }

        private void UpdateFromRemote()
        {
            if (Directory.Exists(remoteConfigsPath))
            {
                using var repo = new Repository(remoteConfigsPath);
                repo.Reset(ResetMode.Hard);
                Commands.Pull(repo, new Signature("MERGE_USER_NAME", "MERGE_USER_EMAIL", DateTimeOffset.Now), null);
            }
            else Repository.Clone(remote, remoteConfigsPath);
        }
    }
}
