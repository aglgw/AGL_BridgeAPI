using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.ApplicationCore.Filters
{
    public class ValidateJsonPropertiesFilter : ActionFilterAttribute
    {
        public async override void OnActionExecuting(ActionExecutingContext context)
        {
            // DisableValidation 특성이 있는 경우 유효성 검사 건너뜀
            if (context.ActionDescriptor.EndpointMetadata.OfType<DisableValidationAttribute>().Any())
            {
                return;
            }

            var dtoArgument = context.ActionArguments.Values.FirstOrDefault(arg => arg != null && !arg.GetType().IsPrimitive && !(arg is string));
            if (dtoArgument == null)
            {
                // DTO가 없으면 필터를 종료
                return;
            }

            // 요청 본문을 여러 번 읽을 수 있도록 설정
            context.HttpContext.Request.EnableBuffering();

            // 요청 본문을 다시 읽기 위해 스트림을 시작 위치로 설정
            context.HttpContext.Request.Body.Position = 0;

            // JSON 데이터에서 요청 본문 읽기
            using var reader = new StreamReader(context.HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            context.HttpContext.Request.Body.Position = 0; // 스트림 위치를 초기화하여 다른 미들웨어가 사용할 수 있도록 함

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            };

            // 요청 본문을 Dictionary로 변환
            Dictionary<string, object> requestData;
            try
            {
                requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(body, options);
            }
            catch (Exception ex)
            {
                context.Result = new JsonResult(new
                {
                    isSuccess = false,
                    rstCd = "ERROR",
                    rstMsg = "Invalid JSON format: " + ex.Message,
                    statusCode = 400
                })
                {
                    StatusCode = 400
                };
                return;
            }

            if (requestData == null)
            {
                // JSON 요청 본문이 없는 경우
                context.Result = new JsonResult(new
                {
                    isSuccess = false,
                    rstCd = "ERROR",
                    rstMsg = "Invalid JSON format.",
                    statusCode = 400
                })
                {
                    StatusCode = 400
                };
                return;
            }

            // DTO에 대해 재귀적으로 Required 필드 검사
            var validationErrors = new List<string>();
            ValidateRequiredProperties(dtoArgument, requestData, validationErrors, dtoArgument.GetType().Name);

            // 누락된 필드가 있을 경우 응답 설정
            if (validationErrors.Any())
            {
                var errorMessage = string.Join("; ", validationErrors);
                context.Result = new JsonResult(new
                {
                    isSuccess = false,
                    rstCd = "ERROR",
                    rstMsg = errorMessage,
                    statusCode = 400
                })
                {
                    StatusCode = 400
                };
            }
        }
        private void ValidateRequiredProperties(object dto, Dictionary<string, object> requestData, List<string> errors, string parentPath)
        {
            var properties = dto.GetType().GetProperties();

            foreach (var property in properties)
            {
                var propertyName = property.Name;

                // Required 속성이 있는지 확인
                var isRequired = Attribute.IsDefined(property, typeof(RequiredAttribute));

                // 해당 프로퍼티가 요청 데이터에 있는지 확인하고 없으면 오류 처리
                if (isRequired && !requestData.ContainsKey(propertyName))
                {
                    errors.Add($"{parentPath}.{propertyName} is required");
                    continue;
                }

                // 해당 프로퍼티가 클래스 타입인 경우 재귀적으로 내부 필드를 검사
                if (requestData.TryGetValue(propertyName, out var propertyValue))
                {
                    if (property.PropertyType == typeof(string) && propertyValue is JsonElement stringElement)
                    {
                        // 문자열 값이면 Trim 처리
                        if (stringElement.ValueKind == JsonValueKind.String)
                        {
                            var trimmedValue = stringElement.GetString()?.Trim();
                            property.SetValue(dto, trimmedValue);
                        }
                    }

                    if (propertyValue is JsonElement nestedData)
                    {
                        try
                        {
                            if (property.GetValue(dto) is IEnumerable<object> collection)
                            {
                                // 리스트나 컬렉션 타입일 경우 각 요소를 검사
                                int index = 0;
                                foreach (var item in collection)
                                {
                                    if (item != null)
                                    {
                                        var nestedCollectionData = nestedData.EnumerateArray().ElementAtOrDefault(index);
                                        if (nestedCollectionData.ValueKind == JsonValueKind.Object)
                                        {
                                            var nestedRequestData = JsonSerializer.Deserialize<Dictionary<string, object>>(nestedCollectionData.GetRawText());
                                            ValidateRequiredProperties(item, nestedRequestData, errors, $"{parentPath}.{propertyName}[{index}]");
                                        }
                                    }
                                    index++;
                                }
                            }
                            else if (property.GetValue(dto) != null && nestedData.ValueKind == JsonValueKind.Object)
                            {
                                // 클래스 타입일 경우 내부 필드 검사
                                var nestedRequestData = JsonSerializer.Deserialize<Dictionary<string, object>>(nestedData.GetRawText());
                                ValidateRequiredProperties(property.GetValue(dto), nestedRequestData, errors, $"{parentPath}.{propertyName}");
                            }
                        }
                        catch (JsonException ex)
                        {
                            errors.Add($"{parentPath}.{propertyName} has invalid JSON format: {ex.Message}");
                        }
                    }
                }
            }
        }

    }
}
