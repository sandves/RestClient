using System.Net;
using System.Net.Http.Headers;

namespace Hest.RestClient
{
    public class Response<T>
    {
        public T Content { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}