using System;
using System.IO;
using LibGit2Sharp;

namespace PCSX2_Configurator.Core
{
    public sealed class RemoteConfigurationService
    {
        private readonly string configsPath;
        private readonly string remote = "https://github.com/Zombeaver/PCSX2-Configs";

        public RemoteConfigurationService(string configsPath)
        {
            this.configsPath = configsPath ?? "remote";

            UpdateFromRemote();
        }

        private void UpdateFromRemote()
        {
            if (Directory.Exists(configsPath))
            {
                using (var repo = new Repository(configsPath))
                {
                    repo.Reset(ResetMode.Hard);
                    Commands.Pull(repo, new Signature("MERGE_USER_NAME", "MERGE_USER_EMAIL", DateTimeOffset.Now), null);
                }
            }
            else Repository.Clone(remote, configsPath);
        }
    }
}
