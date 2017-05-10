using System;
using System.Net;

namespace Hest.RestClient
{
    public class SimpleHttpResponseException : Exception
    {
        public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; private set; }
    }
}