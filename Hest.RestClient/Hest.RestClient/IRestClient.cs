using System.IO;
using System.Threading.Tasks;
using Polly;
using System;

namespace Hest.RestClient
{
    public interface IRestClient : IDisposable
    {
        Policy Policy { get; set; }

        TResult Delete<TResult>(string url, params string[] parameters);
        Task<TResult> DeleteAsync<TResult>(string url, params string[] parameters);
        void EnableDefaultPolicy();
        TResult Get<TResult>(string url, params string[] parameters);
        Task<TResult> GetAsync<TResult>(string url, params string[] parameters);
        Stream GetStream(string url, params string[] parameters);
        Task<Stream> GetStreamAsync(string url, params string[] parameters);
        TResult Post<TResult>(string url, object body);
        TResult Post<TResult>(string url, object body, params string[] parameters);
        Task<TResult> PostAsync<TResult>(string url, object body);
        Task<TResult> PostAsync<TResult>(string url, object body, params string[] parameters);
        TResult Put<TResult>(string url, object body, params string[] parameters);
        Task<TResult> PutAsync<TResult>(string url, object body, params string[] parameters);
    }
}