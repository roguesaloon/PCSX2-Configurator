using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PCSX2_Configurator.Services
{
    internal sealed class RedumpDiscIdLookupService : IDiscIdLookupService
    {
        public async Task<string> LookupDiscId(string discId)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = await htmlWeb.LoadFromWebAsync($"http://redump.org/discs/quicksearch/{discId}/");

            var title = default(string);
            var pageTitle = htmlDocument.DocumentNode.SelectSingleNode("//head/title").InnerText.Split("&bull;").Last().Trim();
            if (pageTitle == "Discs")
            {
                var topResult = htmlDocument.DocumentNode.SelectNodes("//table[@class='games']/tr").FirstOrDefault(row => !row.HasClass("th"));
                title = topResult.SelectSingleNode("td[2]").InnerText;
            }
            else title = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='main']/h1").InnerText;

            title = Regex.Replace(title, @"\(.*?\)", "").Trim();
            return title;
        }
    }
}
