using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Services
{
    internal sealed class SevenZipIdentificationService : BaseIdentificationService
    {
        public SevenZipIdentificationService(IGameLibraryService gameLibraryService, ICoverService coverService, IRemoteConfigService remoteConfigService) : base(gameLibraryService, coverService, remoteConfigService) { }

        public override Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            throw new NotImplementedException();
        }
    }
}
