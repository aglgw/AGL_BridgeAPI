using AGL.Api.ApplicationCore.Models;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace AGL.Api.ApplicationCore.Middleware
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var jsonResult = JsonConvert.SerializeObject(new Failure
                        {
                            Code = ResultCode.Forbidden,
                            rstMsg = contextFeature.Error.Message
                        });

                        if (contextFeature.Error is DomainException)
                        {
                            var domainException = contextFeature.Error as DomainException;
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                            jsonResult = JsonConvert.SerializeObject(new Failure
                            {
                                Code = domainException.Code,
                                rstMsg = domainException.Message
                            });
                        }

                        await context.Response.WriteAsync(jsonResult);
                    }
                });
            });
        }
    }
}
