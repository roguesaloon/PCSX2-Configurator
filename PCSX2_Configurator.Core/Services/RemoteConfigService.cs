using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Services
{
    internal sealed class RemoteConfigService : IRemoteConfigService
    {
        private readonly string remoteConfigsPath;
        private readonly string remote = "https://github.com/Zombeaver/PCSX2-Configs";
        private readonly IConfigurationService configurationService;

        public RemoteConfigService(AppSettings appSettings, IConfigurationService configurationService)
        {
            remoteConfigsPath = appSettings.RemoteConfigsPath ?? "Remote";
            this.configurationService = configurationService;
        }

        public void ImportConfig(string directoryName, string inisPath)
        {
            if (!Directory.GetDirectories(remoteConfigsPath).Any(directory => directory == directoryName)) throw new Exception("Config does not exist");
            var configPath = $"{remoteConfigsPath}\\{directoryName}";
            var configName = Path.GetDirectoryName(configPath);
            var importedConfigPath = configurationService.CreateConfig(configName, inisPath, ConfigOptions.Default);

            foreach (var file in Directory.GetFiles(configPath))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, $"{importedConfigPath}\\{fileName}");
            }
        }

        public void UpdateFromRemote()
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
