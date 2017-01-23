using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Hest.RestClient
{
    public static class PolicyFactory
    {
        public static Policy GetDefaultPolicy()
        {
            return Policy
                    .Handle<WebException>()
                    .Or<HttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.Response.StatusCode))
                    .WaitAndRetryAsync(
                        retryCount: 4,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    );
        }

        public static Policy GetDefaultPolicy(Action<Exception, TimeSpan, Context> onRetry)
        {
            return Policy
                    .Handle<WebException>()
                    .Or<HttpResponseException>(e => !HttpStatusCodesThatShouldNotBeRetried.Contains(e.Response.StatusCode))
                    .WaitAndRetryAsync(
                        retryCount: 4,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: onRetry
                    );
        }

        public static List<HttpStatusCode> HttpStatusCodesWorthRetrying { get; } = new List<HttpStatusCode> {
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
    }
}
