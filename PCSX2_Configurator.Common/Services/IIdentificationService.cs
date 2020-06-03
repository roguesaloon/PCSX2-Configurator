using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCSX2_Configurator.Common;

namespace PCSX2_Configurator.Services
{
    public interface IIdentificationService
    {
        Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath);
        Task ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, Action<GameInfo, string> callback, Action deferredCallback);
    }
}
