using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dasync.Collections;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Helpers;

namespace PCSX2_Configurator.Services
{
    public interface IEmulationService
    {
        internal IProcessHelpers ProcessHelpers { get; }
        internal IFileHelpers FileHelpers { get; }
        void LaunchWithGame(string emulatorPath, string gamePath, string configPath, string launchOptions);
        void LaunchWithConfig(string emulatorPath, string gamePath, string configPath);
        void ConfigureGraphicsPlugin(string emulatorPath, string configPath);
        void ConfigureSoundPlugin(string emulatorPath, string configPath);
        void ConfigureInputPlugin(string emulatorPath, string configPath);
        string GetInisPath(string emulatorPath);
        void EnsureUsingIso(string inisPath);
        Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath);
        async void ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, IGameLibraryService gameLibraryService, ICoverService coverService, Action<GameInfo, string> callback)
        {
            var updateGameInfos = new Queue<Action>();
            var inisPath = GetInisPath(emulatorPath);
            EnsureUsingIso(inisPath);
            FileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", true);

            await gameInfos.ParallelForEachAsync(async gameInfo => {
                if (gameInfo.GameId != null) return;
                var (name, region, id) = await IdentifyGame(emulatorPath, gameInfo.Path);
                var newInfo = new GameInfo(gameInfo) { DisplayName = name, Region = region, GameId = id != "???" ? id : null };
                var cover = await coverService.GetCoverForGame(newInfo);
                updateGameInfos.Enqueue(() => gameLibraryService.UpdateGameInfo(newInfo.Name, newInfo, shouldReloadLibrary: true));

                callback.Invoke(newInfo, cover);
            }, maxDegreeOfParallelism: 20);

            FileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", false);
            while (updateGameInfos.Count > 0) updateGameInfos.Dequeue()?.Invoke();
        }
    }
}
