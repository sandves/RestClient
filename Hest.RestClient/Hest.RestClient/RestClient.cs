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
            var response = await ExecuteAsync(() => GetAsync(requestUri)).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default(TResult);
            return await ReadAsAsync<TResult>(response).ConfigureAwait(false);
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
            return await PostAsync<TResult>(requestUri, body).ConfigureAwait(false);
        }

        public async Task<TResult> PostAsync<TResult>(string url, object body)
        {
            var response = await ExecuteAsync(() => PostAsync(url, body)).ConfigureAwait(false);
            return await ReadAsAsync<TResult>(response).ConfigureAwait(false);
        }

        public async Task<Stream> PostAndReadAsStreamAsync(string url, object body)
        {
            var response = await ExecuteAsync(() => PostAsync(url, body)).ConfigureAwait(false);
            var result = default(Stream);
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            var response = await ExecuteAsync(() => DeleteAsync(requestUri)).ConfigureAwait(false);
            return await ReadAsAsync<TResult>(response).ConfigureAwait(false);
        }

        public async Task<TResult> PutAsync<TResult>(string url, object body, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => PutAsync(requestUri, body)).ConfigureAwait(false);
            return await ReadAsAsync<TResult>(response).ConfigureAwait(false);
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
            var response = await ExecuteAsync(() => PatchAsync(requestUri, body)).ConfigureAwait(false);
            return await ReadAsAsync<TResult>(response).ConfigureAwait(false);
        }

        public async Task<Stream> GetStreamAsync(string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            return await Client.GetStreamAsync(requestUri).ConfigureAwait(false);
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

        public async Task<object> GetAsync(Type type, string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = await ExecuteAsync(() => GetAsync(requestUri)).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            return await ReadAsAsync(response, type).ConfigureAwait(false);
        }

        public object Get(Type type, string url, params object[] parameters)
        {
            var requestUri = string.Format(url, parameters);
            var response = Execute(() => GetAsync(requestUri)).Result;
            return ReadAs(response, type);
        }

        private void InitializeClient()
        {
            Client = new HttpClient(new HttpClientHandler { Credentials = CredentialCache.DefaultNetworkCredentials });
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
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseMessage> PostAsync(string requestUri, object body)
        {
            var response = await Client.PostAsync(requestUri, body, Formatter).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            var response = await Client.DeleteAsync(requestUri).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseMessage> PutAsync(string requestUri, object body)
        {
            var response = await Client.PutAsync(requestUri, body, Formatter).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return response;
        }

        private async Task<HttpResponseMessage> PatchAsync(string requestUri, object body)
        {
            var response = await Client.PatchAsync(requestUri, body, Formatter).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return response;
        }

        private static async Task<TResult> ReadAsAsync<TResult>(HttpResponseMessage response)
        {
            var result = default(TResult);
            if (response.IsSuccessStatusCode)
                result = await response.Content
                    .ReadAsStringAsync()
                    .ContinueWith(task => JsonConvert.DeserializeObject<TResult>(task.Result))
                    .ConfigureAwait(false);
            return result;
        }

        private static async Task<object> ReadAsAsync(HttpResponseMessage response, Type type)
        {
            var result = default(object);
            if (response.IsSuccessStatusCode)
                result = await response.Content
                    .ReadAsStringAsync()
                    .ContinueWith(task => JsonConvert.DeserializeObject(task.Result, type))
                    .ConfigureAwait(false);
            return result;
        }

        private static TResult ReadAs<TResult>(HttpResponseMessage response)
        {
            return ReadAsAsync<TResult>(response).Result;
        }

        private static object ReadAs(HttpResponseMessage response, Type type)
        {
            return ReadAsAsync(response, type).Result;
        }

        private async Task<T> ExecuteAsync<T>(Func<Task<T>> function)
        {
            if (Policy == null)
                return await function.Invoke().ConfigureAwait(false);
            return await Policy.ExecuteAsync(function.Invoke).ConfigureAwait(false);
        }

        private T Execute<T>(Func<T> function)
        {
            return Policy == null ? function.Invoke() : Policy.Execute(function.Invoke);
        }
    }
}