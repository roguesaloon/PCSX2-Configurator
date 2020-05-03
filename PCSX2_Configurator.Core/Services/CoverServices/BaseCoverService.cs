using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    public abstract class BaseCoverService : ICoverService
    {
        protected string CoversPath { get; set; } = $"{Directory.GetCurrentDirectory()}/Assets/Covers";
        private string MissingCoverArt { get; }

        protected static readonly HttpClient httpClient = new HttpClient();

        static BaseCoverService()
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        protected BaseCoverService(string coversPath, string missingCoverArt)
        {
            var rootedPath = Path.IsPathRooted(coversPath) ? coversPath : $"{Directory.GetCurrentDirectory()}/{coversPath}";
            CoversPath = coversPath != null ? rootedPath : CoversPath; 
            MissingCoverArt = missingCoverArt;
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

        protected async static Task<bool> DownloadFile(string source, string destination)
        {
            using var response = await httpClient.GetAsync(source, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) return false;
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Open(destination, FileMode.Create);
            await responseStream.CopyToAsync(fileStream);
            return true;
        }
    }
}
