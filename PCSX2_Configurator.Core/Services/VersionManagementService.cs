using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;
using SevenZip;
using HtmlAgilityPack;
using IniParser;
using IniParser.Model;
using NaturalSort.Extension;
using PCSX2_Configurator.Settings;
using PCSX2_Configurator.Core.Helpers;

namespace PCSX2_Configurator.Core.Services
{
    public sealed class VersionManagementService
    {
        private readonly HttpClient httpClient;
        private readonly VersionManagerSettings settings;
        private readonly AppSettings appSettings;
        private readonly FileIniDataParser iniParser;

        public VersionManagementService(VersionManagerSettings settings, AppSettings appSettings, IHttpClientFactory httpClientFactory)
        {
            this.settings = settings;
            this.appSettings = appSettings;
            httpClient = httpClientFactory.CreateClient();
            
            SevenZipBase.SetLibraryPath(appSettings.SevenZipLibraryPath);
            iniParser = new FileIniDataParser();
        }

        public async Task<IDictionary<string, VersionSettings>> GetAvailableVersions()
        {
            var versions = new List<VersionSettings>();
            versions.AddRange(settings.StableVersions);

            var devVersions = await GetDevVersions();
            versions.AddRange(devVersions);

            var latestDevVersion = GetLatestDevVersion(devVersions);
            versions.Add(latestDevVersion);

            var availableVersions = versions.Where(version => !appSettings.Versions.Any(installed => version.Name == installed.Key));
            return new SortedDictionary<string, VersionSettings>(availableVersions.ToDictionary(x => x.Name), StringComparer.OrdinalIgnoreCase.WithNaturalSort());
        }

        public async Task InstallVersion(VersionSettings version)
        {
            var installPath = await DownloadAndExtractArchive(version);
            ConfigureBiosDirectory(installPath, version.InisDirectory);

            appSettings.Versions.Add(version.Name, $"{Path.GetFullPath(installPath)}\\{version.Executable}");
            await appSettings.UpdateVersions();

            Process.Start($"{installPath}/{version.Executable}");
        }

        public static string GetMostRecentStableVersion(IEnumerable<string> versionNames)
        {
            var sortedVersions = versionNames.OrderBy(version => version, StringComparer.OrdinalIgnoreCase.WithNaturalSort());
            var latestStable = sortedVersions.LastOrDefault(version => !version.Contains("dev"));
            return latestStable ?? sortedVersions.LastOrDefault();
        }

        private async Task<List<VersionSettings>> GetDevVersions()
        {
            var uri = new Uri(settings.DevVersions);
            var htmlDocument = await new HtmlWeb().LoadFromWebAsync(uri.AbsoluteUri);
            var versions = new List<VersionSettings>();

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

                versions.Add(version);
            }

            return versions;
        }

        private VersionSettings GetLatestDevVersion(IEnumerable<VersionSettings> versions)
        {
            var version = versions.First(x => x.IsDevBuild);
            return new VersionSettings
            {
                Name = version.Name.Substring(0, version.Name.LastIndexOf('-')) + "-latest",
                DownloadLink = version.DownloadLink,
                Directory = version.Directory.Substring(0, version.Directory.LastIndexOf(" ")) + " latest",
                ArchiveName = version.ArchiveName,
                ShouldUpdate = true,
                IsDevBuild = true
            };
        }

        private async Task<string> DownloadAndExtractArchive(VersionSettings version)
        {
            var versionsDirectory = Directory.CreateDirectory(settings.VersionsDirectory);
            var archivesDirectory = Directory.CreateDirectory($"{versionsDirectory}\\{settings.ArchivesDirectory}");

            var archive = $"{archivesDirectory}\\{version.ArchiveName}";
            if (!File.Exists(archive)) await httpClient.DownloadFile(version.DownloadLink, archive, referer: "https://pcsx2.net");

            using var extractor = new SevenZipExtractor(archive);
            var rootStructure = extractor.ArchiveFileNames.Where(x => !string.IsNullOrEmpty(x) && !x.Contains("\\"));

            var targetPath = $"{versionsDirectory}\\{version.Directory}";
            if (rootStructure.Count() == 1)
            {
                await extractor.ExtractArchiveAsync($"{versionsDirectory}");
                var extractedPath = $"{versionsDirectory}\\{rootStructure.First()}";

                if (Directory.Exists(extractedPath) && extractedPath != targetPath)
                {
                    Directory.Move(extractedPath, targetPath);
                }
            }
            else await extractor.ExtractArchiveAsync(targetPath);

            return targetPath;
        }

        private void ConfigureBiosDirectory(string installPath, string inisDirectory)
        {
            if (string.IsNullOrWhiteSpace(settings.BiosDirectory)) return;
            var biosDirectory = Directory.CreateDirectory($"{settings.VersionsDirectory}/{settings.BiosDirectory}");
            var config = new IniData();
            config["Folders"]["Bios"] = Path.GetFullPath($"{biosDirectory}").Replace("\\", "\\\\");
            config["Folders"]["UseDefaultBios"] = "disabled";
            var inisPath = Directory.CreateDirectory($"{installPath}/{inisDirectory}");
            iniParser.WriteFile($"{inisPath}/PCSX2_ui.ini", config);
        }
    }
}
