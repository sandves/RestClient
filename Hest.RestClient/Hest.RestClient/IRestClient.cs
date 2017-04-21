using System.IO;
using System.Threading.Tasks;
using Polly;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace Hest.RestClient
{
    public interface IRestClient : IDisposable
    {
        Policy Policy { get; set; }
        HttpClient Client { get; set; }
        MediaTypeFormatter Formatter { get; set; }

        TResult Delete<TResult>(string url, params object[] parameters);
        Task<TResult> DeleteAsync<TResult>(string url, params object[] parameters);
        void EnableDefaultPolicy();
        TResult Get<TResult>(string url, params object[] parameters);
        Task<TResult> GetAsync<TResult>(string url, params object[] parameters);
        Stream GetStream(string url, params object[] parameters);
        Task<Stream> GetStreamAsync(string url, params object[] parameters);
        TResult Post<TResult>(string url, object body);
        TResult Post<TResult>(string url, object body, params object[] parameters);
        Task<TResult> PostAsync<TResult>(string url, object body);
        Task<TResult> PostAsync<TResult>(string url, object body, params object[] parameters);
        TResult Put<TResult>(string url, object body, params object[] parameters);
        Task<TResult> PutAsync<TResult>(string url, object body, params object[] parameters);
        TResult Patch<TResult>(string url, object body, params object[] parameters);
        Task<TResult> PatchAsync<TResult>(string url, object body, params object[] parameters);
        Task<Stream> PostAndReadAsStreamAsync(string url, object body);
    }
}