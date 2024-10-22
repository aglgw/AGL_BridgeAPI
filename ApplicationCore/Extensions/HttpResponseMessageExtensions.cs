using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<string> TryReadAsStringAsync(this HttpResponseMessage httpResponseMessage)
        {
            var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

            switch (httpResponseMessage.StatusCode)
            {
                case HttpStatusCode.OK:
                    return responseString;
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.Unauthorized:
                    throw new DomainException(Models.Enum.ResultCode.잘못된요청, "Unauthorized(StatusCode:401)" + responseString);
                default:
                    throw new Exception(responseString);
            }
        }
    }
}
