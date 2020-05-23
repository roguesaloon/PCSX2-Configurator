using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        (string gameTitle, string gameRegion, string gameId) IdentifyGame(string emulatorPath, string gamePath);
        void ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, IGameLibraryService gameLibraryService, ICoverService coverService, Action<GameInfo, string> callback)
        {
            var updateGameInfos = new Queue<Action>();
            var inisPath = GetInisPath(emulatorPath);
            EnsureUsingIso(inisPath);
            FileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", true);

            var groups = gameInfos
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 20)
                .Select(x => x.Select(v => v.Value));

            foreach (var group in groups)
            {
                Parallel.ForEach(group, async gameInfo =>
                {
                    if (gameInfo.GameId != null) return;
                    var (name, region, id) = IdentifyGame(emulatorPath, gameInfo.Path);
                    var newInfo = new GameInfo(gameInfo) { DisplayName = name, Region = region, GameId = id };
                    var cover = await coverService.GetCoverForGame(newInfo);
                    updateGameInfos.Enqueue(() => gameLibraryService.UpdateGameInfo(newInfo.Name, newInfo, shouldReloadLibrary: true));

                    callback.Invoke(newInfo, cover);
                });
            }

            FileHelpers.SetFileToReadOnly($"{inisPath}/{ConfiguratorConstants.UiFileName}", false);
            while (updateGameInfos.Count > 0) updateGameInfos.Dequeue()?.Invoke();
        }
    }
}
