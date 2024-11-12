using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Filters
{
    public class CustomResponseFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 모델 유효성 검사 오류가 있는 경우 실행
            if (!context.ModelState.IsValid)
            {
                var description = ExtensionMethods.GetDescription(ResultCode.INVALID_INPUT);
                var errorMessage = string.Join("; ", context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).FirstOrDefault());

                // 커스텀 응답 객체 생성
                var customResponse = new
                {
                    isSuccess = false,
                    rstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)ResultCode.INVALID_INPUT}) {errorMessage}",
                    statusCode = 400
                };

                // JSON 응답 설정
                context.Result = new JsonResult(customResponse)
                {
                    StatusCode = 400
                };
            }
            // ModelState가 유효한 경우 비즈니스 로직이 실행됩니다.
        }
    }
}
