using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class Playstation2ArchiveCoverService : BaseCoverService
    {
        private readonly string baseUri = "http://www.atensionspan.com/evil/covers"; // Links From http://www.evilbadman.com/

        public Playstation2ArchiveCoverService(CoverSettings settings, IHttpClientFactory httpClientFactory) : base(settings, httpClientFactory)
        {
            CoversPath = $"{CoversPath}/Playstation2Archive";
        }

        protected override async Task GetCoverForGame(GameInfo game, string targetFile)
        {
            var sourceFile = $"{game.DisplayName}.jpg";
            await DownloadCoverFromSource(sourceFile, targetFile);

            if (File.Exists(targetFile))
            {
                using var image = Image.Load(targetFile);
                var croppedCoverRectangle = GetCroppedCoverRectangle(image.Width, image.Height);
                image.Mutate(x => x.Crop(croppedCoverRectangle));
                image.Save(targetFile);
            }
        }

        private async Task DownloadCoverFromSource(string fileName, string filePath)
        {
            var formattedFileName = FormatNameForService(fileName);
            var sourceFile = $"{baseUri}/{formattedFileName}";
            if (!await httpClient.DownloadFile(sourceFile, filePath)) await RetryDownloadWhereFails(fileName, filePath);
        }

        private async Task RetryDownloadWhereFails(string fileName, string filePath)
        {
            if (!fileName.Contains("(Bonus Disc)"))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName) + " (Bonus Disc).jpg";
                await DownloadCoverFromSource(fileName, filePath);
                return;
            }
            fileName = fileName.Replace(" (Bonus Disc)", "");

            if (fileName.Contains("vs."))
            {
                fileName = fileName.Replace("vs.", "versus");
                await DownloadCoverFromSource(fileName, filePath);
                return;
            }
            if(fileName.Contains("versus"))
            {
                fileName = fileName.Replace("versus", "vs");
                await DownloadCoverFromSource(fileName, filePath);
                return;
            }
            fileName = fileName.Replace("versus", "vs.");

            if (fileName.Contains(" - "))
            {
                fileName = fileName.Replace(" - ", " ");
                await DownloadCoverFromSource(fileName, filePath);
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
