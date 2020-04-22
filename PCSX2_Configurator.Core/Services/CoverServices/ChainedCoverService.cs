using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public sealed class ChainedCoverService : ICoverService
    {
        private readonly List<ICoverService> chainOfCoverServices;

        public ChainedCoverService(string coversPath, string missingCoverArt)
        {
            chainOfCoverServices = new List<ICoverService>
            {
                new PlaystationDataCenterCoverService(coversPath, missingCoverArt),
                new TgdbCoverService(coversPath, missingCoverArt),
                new Playstation2ArchiveCoverService(coversPath, missingCoverArt)
            };
        }

        public async Task<string> GetCoverForGame(GameInfo game)
        {
            var result = default(string);
            foreach(var coverService in chainOfCoverServices)
            {
                result = await coverService.GetCoverForGame(game);
                if (!string.IsNullOrEmpty(result) && !result.ToLower().Contains("missing")) break;
            }
            return result;
        }
    }
}