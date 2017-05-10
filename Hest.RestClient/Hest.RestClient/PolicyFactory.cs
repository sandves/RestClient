using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Polly;

namespace Hest.RestClient
{
    public static class PolicyFactory
    {
        public static List<HttpStatusCode> HttpStatusCodesWorthRetrying { get; } = new List<HttpStatusCode>
        {
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };

        public static List<HttpStatusCode> HttpStatusCodesThatShouldNotBeRetried { get; } = new List<HttpStatusCode>
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized
        };

        public static Policy GetDefaultPolicy()
        {
            return Policy
                .Handle<HttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.Response.StatusCode))
                .Or<SimpleHttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.StatusCode))
                .WaitAndRetryAsync(
                    4,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );
        }

        public static Policy GetDefaultPolicy(Action<Exception, TimeSpan, Context> onRetry)
        {
            return Policy
                .Handle<HttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.Response.StatusCode))
                .Or<SimpleHttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.StatusCode))
                .WaitAndRetryAsync(
                    4,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry
                );
        }
    }
}