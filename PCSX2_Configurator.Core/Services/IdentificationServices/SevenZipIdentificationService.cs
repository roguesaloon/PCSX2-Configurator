using PCSX2_Configurator.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Services
{
    internal sealed class SevenZipIdentificationService : IIdentificationService
    {
        public Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            throw new NotImplementedException();
        }

        public Task ImportGames(string emulatorPath, IEnumerable<GameInfo> gameInfos, Action<GameInfo, string> callback, Action deferredCallback)
        {
            throw new NotImplementedException();
        }
    }
}
