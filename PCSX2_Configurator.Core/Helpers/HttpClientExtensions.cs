using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PCSX2_Configurator.Core.Helpers
{
    internal static class HttpClientExtensions
    {
        public async static Task<bool> DownloadFile(this HttpClient httpClient, string source, string destination, string referer = null)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(source),
                Method = HttpMethod.Get
            };
            if (referer != null) request.Headers.Referrer = new Uri(referer);
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) return false;
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Open(destination, FileMode.Create);
            await responseStream.CopyToAsync(fileStream);
            return true;
        }
    }
}
