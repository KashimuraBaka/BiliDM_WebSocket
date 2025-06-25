using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace BiliDM_WebSocket.Utils
{
    public static class RequestHelper
    {
        public static async Task<T> ReadFromJsonAsync<T>(this HttpContent httpContent)
        {
            var contentString = await httpContent.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(contentString))
            {
                return JsonConvert.DeserializeObject<T>(contentString);
            }
            return default;
        }
    }

    public class Request
    {
        private HttpClient Client { get; }

        public Request()
        {
            Client = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
        }

        public async Task<T> Get<T>(string url)
        {
            try
            {
                var response = await Client.GetAsync(url);
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching data from {url}: {ex.Message}");
                return default;
            }
        }

        public async Task<byte[]> GetData(string url)
        {
            try
            {
                var response = await Client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching data from {url}: {ex.Message}");
                return default;
            }
        }
    }
}
