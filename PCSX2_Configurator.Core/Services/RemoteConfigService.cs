using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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
        private readonly IEmulationService emulationService;
        private readonly XmlDocument remoteIndex;

        public RemoteConfigService(AppSettings appSettings, IConfigurationService configurationService, IEmulationService emulationService)
        {
            remoteConfigsPath = appSettings.RemoteConfigsPath ?? "Remote";
            this.configurationService = configurationService;
            this.emulationService = emulationService;
            remoteIndex = new XmlDocument();

            Task.Run(UpdateFromRemote).ContinueWith(task => remoteIndex.Load($"{remoteConfigsPath}\\RemoteIndex.xml"));
        }

        public void ImportConfig(string gameId, string emulatorPath)
        {
            var configElement = remoteIndex.SelectSingleNode($"//Config[GameIds/GameId = '{gameId}']") as XmlElement;
            var configDirectory = configElement.GetAttribute("Name");
            var availableConfigs = Directory.GetDirectories($"{remoteConfigsPath}\\Game Configs").Select(dir => Path.GetFileName(dir));
            if (!availableConfigs.Any(directory => directory == configDirectory)) return;
            var configPath = $"{remoteConfigsPath}\\Game Configs\\{configDirectory}";
            var configName = Regex.Replace(configDirectory, "id#\\d+", "").Trim().ToLowerInvariant().Replace(" ", "-");
            var inisPath = emulationService.GetInisPath(emulatorPath);
            var importedConfigPath = configurationService.CreateConfig(configName, inisPath, ConfigOptions.Default);

            // Additional Merge Operations, copy is placeholder
            foreach (var file in Directory.GetFiles(configPath))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, $"{importedConfigPath}\\{fileName}", overwrite: true);
            }
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
