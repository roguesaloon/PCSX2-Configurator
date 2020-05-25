using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Services
{
    internal sealed class SevenZipIdentificationService : BaseIdentificationService
    {
        public SevenZipIdentificationService(IGameLibraryService gameLibraryService, ICoverService coverService) : base(gameLibraryService, coverService) { }

        public override Task<(string gameTitle, string gameRegion, string gameId)> IdentifyGame(string emulatorPath, string gamePath)
        {
            throw new NotImplementedException();
        }
    }
}
