using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace YouTubeDLHandler
{
    internal static class Downloader
    {
        private static readonly HttpClient client = new();

        // Borrowed from: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-7.0
        private static async Task DownloadAsync(string url, string destination)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                using HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseBody);
                // Borrowed from: https://stackoverflow.com/a/54475013/1112800
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                using (var fs = new FileStream(destination, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }
            catch (HttpRequestException e)
            {
                Logger.WriteErrorLine("\nException Caught!\n\nMessage: {0}", e.Message);
            }
        }

        internal static void Download(string url, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? Path.Join(Program.ASSEMBLY_LOCATION, "downloads"));
            Task.Run(async () => await DownloadAsync(url, destination));
        }
    }
}
