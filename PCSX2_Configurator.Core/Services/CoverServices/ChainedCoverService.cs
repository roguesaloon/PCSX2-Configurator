using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public sealed class ChainedCoverService : ICoverService
    {
        private readonly List<ICoverService> chainOfCoverServices;

        public ChainedCoverService(string coversPath)
        {
            chainOfCoverServices = new List<ICoverService>
            {
                new PlaystationDataCenterCoverService(coversPath),
                new TgdbCoverService(coversPath),
                new Playstation2ArchiveCoverService(coversPath)
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