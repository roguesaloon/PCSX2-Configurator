using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace PCSX2_Configurator.Core
{
    public sealed class RemoteConfigurationService
    {
        private readonly string configsPath;
        private readonly string remote = "https://github.com/Zombeaver/PCSX2-Configs";
        private readonly ConfigurationService configurationService;

        public RemoteConfigurationService(ConfigurationService configurationService, string configsPath)
        {
            this.configsPath = configsPath ?? "remote";
            this.configurationService = configurationService;

            UpdateFromRemote();
        }

        public void ImportConfig(string directoryName, string inisPath)
        {
            if (!Directory.GetDirectories(configsPath).Any(directory => directory == directoryName)) throw new Exception("Config does not exist");
            configurationService.ImportConfig($"{configsPath}\\{directoryName}", inisPath, ConfigurationService.SettingsOptions.All);
        }

        private void UpdateFromRemote()
        {
            if (Directory.Exists(configsPath))
            {
                using var repo = new Repository(configsPath);
                repo.Reset(ResetMode.Hard);
                Commands.Pull(repo, new Signature("MERGE_USER_NAME", "MERGE_USER_EMAIL", DateTimeOffset.Now), null);
            }
            else Repository.Clone(remote, configsPath);
        }


    }
}
