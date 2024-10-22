using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Models;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AGL.Api.ApplicationCore.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;
        //private readonly IIdentityService _identityService;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env,
            //IIdentityService identityService,
            ILogger<HttpGlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
            //_identityService = identityService;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception, 
                context.Exception.Message);

            if (context.Exception.GetType() == typeof(DomainException))
            {
                var domainException = context.Exception as DomainException;
                
                var json = new Failure
                {
                    Code = domainException.Code,
                    RstMsg = domainException.Message
                };

                if (_env.IsDevelopment())
                {
                    json.More = context.Exception;
                }

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new Failure
                {
                    Code = ResultCode.SERVER_ERROR,
                    RstMsg = context.Exception.Message
                };

                if (_env.IsDevelopment())
                {
                    json.More = context.Exception;
                }

                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.ExceptionHandled = true;
        }

    }
}
