using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Core
{
    internal static class HttpClientExtensions
    {
        public async static Task<bool> DownloadFile(this HttpClient httpClient, string source, string destination)
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
