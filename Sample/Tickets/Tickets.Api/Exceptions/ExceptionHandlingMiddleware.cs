using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tickets.Api.Exceptions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger logger;

        public ExceptionHandlingMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<ExceptionHandlingMiddleware>();
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            logger.LogError(exception, exception.Message);

            var codeInfo = ExceptionToHttpStatusMapper.Map(exception);

            var result = JsonConvert.SerializeObject(new HttpExceptionWrapper((int)codeInfo.Code, codeInfo.Message));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)codeInfo.Code;
            return context.Response.WriteAsync(result);
        }
    }
}
