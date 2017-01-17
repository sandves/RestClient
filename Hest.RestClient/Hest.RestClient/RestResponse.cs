using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Hest.RestClient
{
    public class RestResponse<TResponse>
    {
        public TResponse Data;
        public HttpResponseHeaders Headers;
        public HttpStatusCode StatusCode;
    }
}
