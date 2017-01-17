using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Polly;

namespace Hest.RestClient
{
    public interface IRestClient
    {
        Policy DefaultPolicy { get; }
        List<HttpStatusCode> HttpStatusCodesThatShouldNotBeRetried { get; set; }
        List<HttpStatusCode> HttpStatusCodesWorthRetrying { get; set; }
        Policy Policy { get; set; }

        TResult Delete<TResult>(string url, params string[] parameters);
        Task<TResult> DeleteAsync<TResult>(string url, params string[] parameters);
        void Dispose();
        void EnableDefaultPolicy();
        TResult Get<TResult>(string url, params string[] parameters);
        Task<RestResponse<TResult>> GetAsync<TResult>(string url, params string[] parameters);
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