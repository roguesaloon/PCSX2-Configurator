using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PCSX2_Configurator.Settings;

namespace PCSX2_Configurator.Core
{
    public sealed class TgdbCoverService : BaseCoverService
    {
        private readonly string apiKey = "MjU1M2RmNWZlY2UzMjhlZWVkYTk2NWJkZjJkNzUwNzM0NjdlY2I0NjE1N2NhNTQzMjI5ZTE0YTMzYzliNWQ4ZQ==";
        private readonly string byGameNameApiUri = "https://api.thegamesdb.net/v1.1/Games/ByGameName?apikey={apiKey}&filter%5Bplatform%5D=11&name=";
        private readonly string cdnThumbsBoxFrontUri = "https://cdn.thegamesdb.net/images/thumb/boxart/front/";

        public TgdbCoverService(CoverSettings settings, IHttpClientFactory httpClientFactory) : base(settings, httpClientFactory)
        {
            CoversPath = $"{CoversPath}/TGDB";
            byGameNameApiUri = byGameNameApiUri.Replace("{apiKey}", Encoding.ASCII.GetString(Convert.FromBase64String(apiKey)));
        }

        protected override async Task GetCoverForGame(GameInfo game, string targetFile)
        {
            var gameId = await GetGameIdFromApi(game.DisplayName);
            if(gameId != null)
            {
                for(int i = 1; i < 9; ++i)
                {
                    var hasDownloaded = await httpClient.DownloadFile($"{cdnThumbsBoxFrontUri}/{gameId}-{i}.jpg", targetFile);
                    if (hasDownloaded) break;
                }
            }
        }

        private async Task<long?> GetGameIdFromApi(string gameName)
        {
            var formattedName = FormatNameForService(gameName);
            using var response = await httpClient.GetAsync($"{byGameNameApiUri}{formattedName}");
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await  response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(responseJson);

            var resultCount = (int) data?.data?.count;
            var games = data?.data?.games as IEnumerable<dynamic>;

            if (resultCount == 0) return null;
            if (resultCount == 1) return games?.FirstOrDefault()?.id;

            return games?.FirstOrDefault(game =>
            {
                var dbTitle = ((string)game.game_title).ToLowerInvariant().Replace(" - ", " ").Replace(":", "");
                var localTitle = formattedName.ToLowerInvariant().Replace(" - ", " ").Replace(":", "");
                return dbTitle == localTitle ? true : dbTitle.Contains(localTitle);
            })?.id;
        }

        private string FormatNameForService(string name)
        {
            name = ApplyPerGameFixes(name);
            name = name.Contains(", The") ? "The " + name.Replace(", The", "") : name;
            name = Regex.Replace(name, "\\[.*?\\]", "").Trim();
            return name;

            static string ApplyPerGameFixes(string name)
            {
                name = name.Replace("dot Hack -", ".hack");
                return name;
            }
        }
    }
}
