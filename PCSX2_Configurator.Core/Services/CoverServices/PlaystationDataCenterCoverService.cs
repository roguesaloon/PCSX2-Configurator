using System.IO;
using System.Net;

namespace PCSX2_Configurator.Core
{
    public sealed class PlaystationDataCenterCoverService : ICoverService
    {
        private readonly string baseUri = "https://psxdatacenter.com/psx2/images2/covers/";
        private readonly string missingCoverArt;
        private readonly string coversPath;

        public PlaystationDataCenterCoverService(string coversPath)
        {
            coversPath ??= "Assets/Covers";
            this.coversPath = $"{coversPath}/PlaystationDataCenter";
            missingCoverArt = $"{coversPath}/Missing.png";
        }

        public string GetCoverForGame(GameInfo game)
        {
            var fileName = $"{game.GameId}.jpg";
            var filePath = $"{Directory.GetCurrentDirectory()}/{coversPath}/{game.GameId}.jpg";
            var missingFilePath = $"{Path.GetDirectoryName(filePath)}/{game.GameId}.missing";
            if (File.Exists(filePath)) return filePath;
            if (File.Exists(missingFilePath)) return missingCoverArt;
            if (!Directory.Exists(coversPath)) Directory.CreateDirectory(coversPath);

            try
            {
                using var webClient = new WebClient { BaseAddress = baseUri };
                webClient.DownloadFile(fileName, filePath);
            }
            catch(WebException e)
            {
                if (e.Status != WebExceptionStatus.ProtocolError) throw;
            }

            if (File.Exists(filePath)) return filePath;
            File.Create(missingFilePath);
            return missingCoverArt;
        }
    }
}
