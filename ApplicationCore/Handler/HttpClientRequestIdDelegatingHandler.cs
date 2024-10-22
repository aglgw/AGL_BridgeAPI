using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Handler
{
    public class HttpClientRequestIdDelegatingHandler
       : DelegatingHandler
    {

        public HttpClientRequestIdDelegatingHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 요청id 헤더 삽입
            if (!request.Headers.Contains("x-requestid"))
            {
                request.Headers.Add("x-requestid", Guid.NewGuid().ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
