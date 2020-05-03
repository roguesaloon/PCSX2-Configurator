using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public sealed class PlaystationDataCenterCoverService : BaseCoverService
    {
        private readonly string baseUri = "https://psxdatacenter.com/psx2/images2/covers";

        public PlaystationDataCenterCoverService(string coversPath, string missingCoverArt) : base(coversPath, missingCoverArt)
        {
            CoversPath = $"{CoversPath}/PlaystationDataCenter";
        }

        protected override async Task GetCoverForGame(GameInfo game, string targetFile)
        {
            var sourceFile = $"{baseUri}/{game.GameId}.jpg";
            await DownloadFile(sourceFile, targetFile);
        }
    }
}