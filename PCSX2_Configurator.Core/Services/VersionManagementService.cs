using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SevenZip;
using HtmlAgilityPack;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class VersionManagementService
    {
        private readonly HttpClient httpClient;
        private readonly VersionManagerSettings settings;

        public VersionManagementService(VersionManagerSettings settings, IHttpClientFactory httpClientFactory, AppSettings appSettings)
        {
            httpClient = httpClientFactory.CreateClient();
            this.settings = settings;
            SevenZipBase.SetLibraryPath(appSettings.SevenZipLibraryPath);
        }

        public async Task<IDictionary<string, VersionSettings>> GetAvailableVersions()
        {
            var availableVersions = new SortedDictionary<string, VersionSettings>();
            foreach (var version in settings.StableVersions)
            {
                availableVersions.Add(version.Name, version);
            }

            var uri = new Uri(settings.DevVersions);
            var htmlDocument = await new HtmlWeb().LoadFromWebAsync(uri.AbsoluteUri);

            var tableNodes = htmlDocument.DocumentNode.SelectNodes("//table[@class='listing']/tr");
            foreach (var node in tableNodes.Where(node => node.Descendants().Any(x => x.Name == "td")))
            {
                var cells = node.Descendants().Where(x => x.Name == "td");
                var name = cells.First().InnerText;
                var query = HttpUtility.HtmlDecode(cells.ElementAt(3).FirstChild.GetAttributeValue("href", string.Empty));

                if (string.IsNullOrWhiteSpace(query)) continue;

                var commitRefIndex = name.LastIndexOf("-g");
                var version = new VersionSettings
                {     
                    Name = name.Substring(0, commitRefIndex < 0 ? name.Length : commitRefIndex),
                    DownloadLink = $"{uri.Scheme}://{uri.Host}{query}",
                    IsDevBuild = true
                };


                var headResponse = await httpClient.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Head,
                    RequestUri = new Uri(version.DownloadLink),
                });
                headResponse.EnsureSuccessStatusCode();

                version.Directory = "PCSX2 " + new StringBuilder(version.Name) { [version.Name.LastIndexOf('-')] = ' ' }.ToString().Substring(1);
                version.ArchiveName = headResponse.Content.Headers.ContentDisposition.FileName.Replace("\"", "");

                availableVersions.Add(version.Name, version);
            }

            var latestBuild = availableVersions.Last(x => x.Value.IsDevBuild).Value;
            latestBuild.Name = latestBuild.Name.Substring(0, latestBuild.Name.LastIndexOf('-')) + "-latest";
            latestBuild.Directory = latestBuild.Directory.Substring(0, latestBuild.Directory.LastIndexOf(" ")) + " latest";
            latestBuild.ShouldUpdate = true;
            availableVersions.Add(latestBuild.Name, latestBuild);

            return availableVersions;
        }

        public async Task InstallVersion(VersionSettings version)
        {
            var versionsDirectory = Directory.CreateDirectory(settings.VersionsDirectory);
            var archivesDirectory = Directory.CreateDirectory($"{versionsDirectory}/{settings.ArchivesDirectory}");
            var biosDirectory = Directory.CreateDirectory($"{versionsDirectory}/{settings.BiosDirectory}");

            var archive = $"{archivesDirectory}/{version.ArchiveName}";
            if (!File.Exists(archive)) await httpClient.DownloadFile(version.DownloadLink, archive);

            using var extractor = new SevenZipExtractor(archive);
            await extractor.ExtractArchiveAsync($"{versionsDirectory}");

            var extractedPath = $"{versionsDirectory}/{Path.GetFileNameWithoutExtension(archive)}";
            var targetPath = $"{versionsDirectory}/{version.Directory}";
            if (Directory.Exists(extractedPath))
            {
                Directory.Move(extractedPath, targetPath);
            }

            Directory.CreateDirectory($"{targetPath}/Bios");
            foreach (var biosFile in biosDirectory.GetFiles())
            {
                File.Copy($"{biosFile}", $"{targetPath}/Bios/{biosFile.Name}");
            }
        }
    }
}
