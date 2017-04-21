using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace Hest.RestClient
{
    public class RestClient : IRestClient
    {
        public RestClient()
        {
            InitializeClient();
        }

        public RestClient(Policy policy)
        {
            Policy = policy;
            InitializeClient();
        }

        public HttpClient Client { get; set; }
        public Policy Policy { get; set; }

        public MediaTypeFormatter Formatter { get; set; }

        public void EnableDefaultPolicy()
        {
            Policy = PolicyFactory.GetDefaultPolicy();
        }

        public async Task<TResult> GetAsync<TResult>(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => GetAsync(requestUri));
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default(TResult);
            return await ReadAsAsync<TResult>(response);
        }

        public TResult Get<TResult>(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = Execute(() => GetAsync(requestUri)).Result;
            return ReadAs<TResult>(response);
        }

        public async Task<TResult> PostAsync<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            return await PostAsync<TResult>(requestUri, body);
        }

        public async Task<TResult> PostAsync<TResult>(string url, object body)
        {
            var response = await ExecuteAsync(() => PostAsync(url, body));
            return await ReadAsAsync<TResult>(response);
        }

        public async Task<Stream> PostAndReadAsStreamAsync(string url, object body)
        {
            var response = await ExecuteAsync(() => PostAsync(url, body));
            var result = default(Stream);
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStreamAsync();
            return result;
        }

        public TResult Post<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            return Post<TResult>(requestUri, body);
        }

        public TResult Post<TResult>(string url, object body)
        {
            var response = Execute(() => PostAsync(url, body)).Result;
            return ReadAs<TResult>(response);
        }

        public TResult Delete<TResult>(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = Execute(() => DeleteAsync(requestUri)).Result;
            return ReadAs<TResult>(response);
        }

        public async Task<TResult> DeleteAsync<TResult>(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => DeleteAsync(requestUri));
            return await ReadAsAsync<TResult>(response);
        }

        public async Task<TResult> PutAsync<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => PutAsync(requestUri, body));
            return await ReadAsAsync<TResult>(response);
        }

        public TResult Put<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = Execute(() => PutAsync(requestUri, body)).Result;
            return ReadAs<TResult>(response);
        }

        public TResult Patch<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = Execute(() => PatchAsync(requestUri, body)).Result;
            return ReadAs<TResult>(response);
        }

        public async Task<TResult> PatchAsync<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => PatchAsync(requestUri, body));
            return await ReadAsAsync<TResult>(response);
        }

        public async Task<Stream> GetStreamAsync(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            return await Client.GetStreamAsync(requestUri);
        }

        public Stream GetStream(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            return Client.GetStreamAsync(requestUri).Result;
        }

        public void Dispose()
        {
            Client.Dispose();
            GC.SuppressFinalize(this);
        }

        private void InitializeClient()
        {
            Client = new HttpClient(new HttpClientHandler {Credentials = CredentialCache.DefaultNetworkCredentials});
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver()
                }
            };
        }

        private async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var response = await Client.GetAsync(requestUri).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return response;
            await response.EnsureSuccessStatusCodeAsync();
            return response;
        }

        private async Task<HttpResponseMessage> PostAsync(string requestUri, object body)
        {
            var response = await Client.PostAsync(requestUri, body, Formatter).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync();
            return response;
        }

        private async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            var response = await Client.DeleteAsync(requestUri);
            await response.EnsureSuccessStatusCodeAsync();
            return response;
        }

        private async Task<HttpResponseMessage> PutAsync(string requestUri, object body)
        {
            var response = await Client.PutAsync(requestUri, body, Formatter);
            await response.EnsureSuccessStatusCodeAsync();
            return response;
        }

        private async Task<HttpResponseMessage> PatchAsync(string requestUri, object body)
        {
            var response = await Client.PatchAsync(requestUri, body, Formatter);
            await response.EnsureSuccessStatusCodeAsync();
            return response;
        }

        private static async Task<TResult> ReadAsAsync<TResult>(HttpResponseMessage response)
        {
            var result = default(TResult);
            if (response.IsSuccessStatusCode)
                result = await response.Content
                    .ReadAsStringAsync()
                    .ContinueWith(task => JsonConvert.DeserializeObject<TResult>(task.Result));
            return result;
        }

        private static TResult ReadAs<TResult>(HttpResponseMessage response)
        {
            return ReadAsAsync<TResult>(response).Result;
        }

        private async Task<T> ExecuteAsync<T>(Func<Task<T>> function)
        {
            if (Policy == null)
                return await function.Invoke();
            return await Policy.ExecuteAsync(function.Invoke);
        }

        private T Execute<T>(Func<T> function)
        {
            return Policy == null ? function.Invoke() : Policy.Execute(function.Invoke);
        }
    }
}