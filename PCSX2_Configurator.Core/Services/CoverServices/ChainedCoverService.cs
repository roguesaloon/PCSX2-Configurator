using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Services
{
    internal sealed class ChainedCoverService : ICoverService
    {
        private readonly List<ICoverService> chainOfCoverServices;

        public ChainedCoverService(AppSettings settings, IHttpClientFactory httpClientFactory)
        {
            chainOfCoverServices = new List<ICoverService>
            {
                new PlaystationDataCenterCoverService(settings, httpClientFactory),
                new TgdbCoverService(settings, httpClientFactory),
                new Playstation2ArchiveCoverService(settings, httpClientFactory)
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