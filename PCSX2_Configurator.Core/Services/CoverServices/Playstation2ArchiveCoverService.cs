using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace PCSX2_Configurator.Core
{
    public sealed class Playstation2ArchiveCoverService : ICoverService
    {
        private readonly string baseUri = "http://www.atensionspan.com/evil/covers/"; // Links From http://www.evilbadman.com/
        private readonly string missingCoverArt;
        private readonly string coversPath;

        public Playstation2ArchiveCoverService(string coversPath)
        {
            coversPath ??= "Assets/Covers";
            this.coversPath = $"{coversPath}/Playstation2Archive";
            missingCoverArt = $"{coversPath}/Missing.png";
        }

        public string GetCoverForGame(GameInfo game)
        {
            var fileName = $"{game.DisplayName}.jpg";
            var filePath = $"{Directory.GetCurrentDirectory()}/{coversPath}/{game.GameId}.jpg";
            var missingFilePath = $"{Path.GetDirectoryName(filePath)}/{game.GameId}.missing";
            if (File.Exists(filePath)) return filePath;
            if (File.Exists(missingFilePath)) return missingCoverArt;
            if (!Directory.Exists(coversPath)) Directory.CreateDirectory(coversPath);


            DownloadCoverFromSource(fileName, filePath);

            if (File.Exists(filePath))
            {
                using var image = Image.Load(filePath);
                var croppedCoverRectangle = GetCroppedCoverRectangle(image.Width, image.Height);
                image.Mutate(x => x.Crop(croppedCoverRectangle));
                image.Save(filePath);

                return filePath;
            }

            if (File.Exists(filePath)) return filePath;
            File.Create(missingFilePath);
            return missingCoverArt;
        }

        private void DownloadCoverFromSource(string fileName, string filePath)
        {
            // Also Need Async Here Way Too Slow
            try
            {
                var formattedName = FormatNameForService(fileName);
                using var webClient = new WebClient { BaseAddress = baseUri };
                webClient.DownloadFile(formattedName, filePath);
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.ProtocolError) throw;
                RetryDownloadWhereFails(fileName, filePath);
            }
        }

        private void RetryDownloadWhereFails(string fileName, string filePath)
        {
            if (!fileName.Contains("(Bonus Disc)"))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + " (Bonus Disc).jpg";
                DownloadCoverFromSource(fileName, filePath);
                return;
            }
            fileName = fileName.Replace(" (Bonus Disc)", "");

            if (fileName.Contains("vs."))
            {
                fileName = fileName.Replace("vs.", "versus");
                DownloadCoverFromSource(fileName, filePath);
                return;
            }
            if(fileName.Contains("versus"))
            {
                fileName = fileName.Replace("versus", "vs");
                DownloadCoverFromSource(fileName, filePath);
                return;
            }
            fileName = fileName.Replace("versus", "vs.");

            if (fileName.Contains(" - "))
            {
                fileName = fileName.Replace(" - ", " ");
                DownloadCoverFromSource(fileName, filePath);
                return;
            }
        }

        private Rectangle GetCroppedCoverRectangle(int width, int height)
        {
            var spine = (width * 5.1f) / 100;
            var fromRight = (int)((width * 0.5f) - (spine * 0.5f));
            return new Rectangle(width - fromRight, 0, fromRight, height);
        }

        private string FormatNameForService(string name)
        {
            name = ApplyPerGameFixes(name);
            name = name.Replace(":", "-");
            name = name.Replace(" -", "-");
            name = Regex.Replace(name, "\\[.*?\\]", "");
            name = Path.GetFileNameWithoutExtension(name).Trim() + ".jpg";
            name = HttpUtility.UrlPathEncode(name);
            return name;

            static string ApplyPerGameFixes(string name)
            {
                name = name.Contains("dot Hack") ? name.Replace("dot Hack -", ".hack").Replace(" -", "%20-") : name;
                name = name.Replace("Dragon Quest", "DragonQuest");
                name = name.Replace("The Return of the King", "Return of the King");
                name = name.Replace("[Greatest Hits]", "(GH)");
                return name;
            }
        }
    }
}
