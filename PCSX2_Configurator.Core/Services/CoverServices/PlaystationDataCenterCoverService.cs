using System.Net.Http;
using System.Threading.Tasks;
using PCSX2_Configurator.Common;
using PCSX2_Configurator.Helpers;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Services
{
    internal sealed class PlaystationDataCenterCoverService : BaseCoverService
    {
        private readonly string baseUri = "https://psxdatacenter.com/psx2/images2/covers";

        public PlaystationDataCenterCoverService(AppSettings settings, IHttpClientFactory httpClientFactory) : base(settings, httpClientFactory)
        {
            CoversPath = $"{CoversPath}/PlaystationDataCenter";
        }

        protected override async Task GetCoverForGame(GameInfo game, string targetFile)
        {
            var sourceFile = $"{baseUri}/{game.GameId}.jpg";
            await httpClient.DownloadFile(sourceFile, targetFile);
        }
    }
}