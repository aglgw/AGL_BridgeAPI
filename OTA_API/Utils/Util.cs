using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AGL.Api.OTA_API.Utils
{
    public class Util
    {
        public static class ResponseUtil
        {
            /// <summary>
            /// 상태 코드에 따라 적절한 HTTP 응답을 반환합니다.
            /// </summary>
            /// <typeparam name="T">응답 데이터 타입</typeparam>
            /// <param name="response">OAPIDataResponse 객체</param>
            /// <returns>ActionResult로 분기된 응답</returns>
            public static ActionResult HandleResponse(IOTAResult result)
            {
                return result.statusCode switch
                {
                    200 => new OkObjectResult(result),
                    400 => new BadRequestObjectResult(result),
                    404 => new NotFoundObjectResult(result),
                    500 => new ObjectResult(result) { StatusCode = 500 },
                    _ => new ObjectResult(result) { StatusCode = result.statusCode } // 기타 상태 코드
                };
            }
        }

    }
}
