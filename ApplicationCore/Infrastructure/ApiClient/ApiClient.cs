using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Infrastructure.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        
        public ApiClient(
            ILogger<ApiClient> logger,
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public HttpClient HttpClient
        {
            get
            {
                return _httpClient;
            }
        }

       
    }
}
