using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace AGL.Api.ApplicationCore.Filters
{
    public class HttpResponseExceptionFilter : IActionFilter
    {
        private readonly ILogger _logger;

        public HttpResponseExceptionFilter(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger("ActionFilterLogger");
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is HttpResponseException exception)
            {
                context.Result = new ObjectResult(exception.Value)
                {
                    StatusCode = exception.Status,
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
    