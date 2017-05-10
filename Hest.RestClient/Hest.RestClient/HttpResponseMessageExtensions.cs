using System.Net.Http;
using System.Threading.Tasks;

namespace Hest.RestClient
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var content = await response.Content.ReadAsStringAsync();

            response.Content?.Dispose();
            throw new SimpleHttpResponseException(response.StatusCode, content);
        }

        public static void EnsureSuccessStatusCode(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;
            var content = response.Content.ReadAsStringAsync().Result;

            response.Content?.Dispose();

            throw new SimpleHttpResponseException(response.StatusCode, content);
        }
    }
}