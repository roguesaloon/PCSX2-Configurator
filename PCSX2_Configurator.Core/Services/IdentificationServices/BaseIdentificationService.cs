using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dasync.Collections;
using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Services
{
    public abstract class BaseIdentificationService : IIdentificationService
    {
        private readonly IGameLibraryService gameLibraryService;
        private readonly ICoverService coverService;

        protected BaseIdentificationService(IGameLibraryService gameLibraryService, ICoverService coverService)
        {
            this.gameLibraryService = gameLibraryService;
            this.coverService = coverService;
        }

        public abstract Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath);

        public virtual async Task ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, Action<GameInfo, string> callback)
        {
            var updateGameInfos = new Queue<Action>();

            await gameInfos.ParallelForEachAsync(async gameInfo => {
                if (gameInfo.GameId != null) return;
                var (name, region, id) = await IdentifyGame(emulatorPath, gameInfo.Path);
                var newInfo = new GameInfo(gameInfo) { DisplayName = name, Region = region, GameId = id != "???" ? id : null };
                var cover = await coverService.GetCoverForGame(newInfo);
                updateGameInfos.Enqueue(() => gameLibraryService.UpdateGameInfo(newInfo.Name, newInfo, shouldReloadLibrary: true));

                callback.Invoke(newInfo, cover);
            }, maxDegreeOfParallelism: 20);

            while (updateGameInfos.Count > 0) updateGameInfos.Dequeue()?.Invoke();
        }
    }
}
