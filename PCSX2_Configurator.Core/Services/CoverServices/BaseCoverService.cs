using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core.Services
{
    public abstract class BaseCoverService : ICoverService
    {
        protected string CoversPath { get; set; } = $"{Directory.GetCurrentDirectory()}/Assets/Covers";
        private string MissingCoverArt { get; }

        protected readonly HttpClient httpClient;

        protected BaseCoverService(CoverSettings settings, IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            CoversPath = settings.CoversPath;
            MissingCoverArt = settings.MissingCover;
        }

        public async Task<string> GetCoverForGame(GameInfo game)
        {
            var existingCover = FindExistingCoverForGame(game);
            if (game.GameId == null) return MissingCoverArt;
            if (existingCover != null) return existingCover;
            var targetFile = $"{CoversPath}/{game.GameId}.jpg";
            await GetCoverForGame(game, targetFile);
            return CoverArtOrMissing(targetFile);
        }

        protected abstract Task GetCoverForGame(GameInfo game, string targetFile);
        private string FindExistingCoverForGame(GameInfo game)
        {
            var filePath = $"{CoversPath}/{game.GameId}.jpg";
            var missingFilePath = $"{CoversPath}/{game.GameId}.missing";
            if (File.Exists(filePath)) return filePath;
            if (File.Exists(missingFilePath)) return MissingCoverArt;
            if (!Directory.Exists(CoversPath)) Directory.CreateDirectory(CoversPath);
            return null;
        }

        private string CoverArtOrMissing(string filePath)
        {
            if (File.Exists(filePath)) return filePath;
            var missingFilePath = $"{Path.GetDirectoryName(filePath)}/{Path.GetFileNameWithoutExtension(filePath)}.missing";
            File.Create(missingFilePath);
            return MissingCoverArt;
        }
    }
}
