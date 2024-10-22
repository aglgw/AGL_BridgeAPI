using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILoggerFactory logger)
        {
            _next = next;
            _logger = logger.CreateLogger("ActionFilterLogger");
        }

        // IMessageWriter is injected into InvokeAsync
        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Method.ToLower() != "options")
            {
                string log = $"Method: \"{httpContext.Request.Method}\", Path: \"{httpContext.Request.Path}\"";
                string data = string.Empty;

                if (httpContext.Request.Method.ToLower() != "get")
                {
                    // Request.Body 복사하기 명령어 추가
                    // Url https://stackoverflow.com/questions/1466804/is-it-possible-to-copy-clone-httpcontext-of-a-web-request
                    httpContext.Request.EnableBuffering();
                    httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                    Stream requestBody = httpContext.Request.Body;
                    byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
                    await requestBody.ReadAsync(buffer, 0, buffer.Length);
                    data = Encoding.UTF8.GetString(buffer);

                    httpContext.Request.Body.Seek(0, SeekOrigin.Begin);

                    if (!string.IsNullOrEmpty(data))
                    {
                        log += $", Body: {data}";
                        _logger.LogInformation(log);
                    }
                }
                else
                {
                    if (httpContext.Request.Query.Count > 0)
                    {
                        foreach (var queryData in httpContext.Request.Query)
                        {
                            data += $"{queryData.Key}: {queryData.Value},";
                        }

                        log += ", Query: { " + data.Substring(0, data.Length - 1) + " }";
                    }

                    _logger.LogInformation(log);
                }
            }

            await _next(httpContext);
        }
    }

    public static class RequestLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggerMiddleware>();
        }
    }
}
