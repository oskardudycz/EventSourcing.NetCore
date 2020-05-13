using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Marten.Exceptions;
using Npgsql;

namespace SmartHome.Api.Exceptions
{
    public class HttpStatusCodeInfo
    {
        public HttpStatusCode Code { get; }
        public string Message { get; }

        public HttpStatusCodeInfo(HttpStatusCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public static HttpStatusCodeInfo Create(HttpStatusCode code, string message)
        {
            return new HttpStatusCodeInfo(code, message);
        }
    }

    public static class ExceptionToHttpStatusMapper
    {
        public static HttpStatusCodeInfo Map(Exception exception)
        {
            var code = exception switch
            {
                UnauthorizedAccessException _ => HttpStatusCode.Unauthorized,
                NotImplementedException _ => HttpStatusCode.NotImplemented,
                InvalidOperationException _ => HttpStatusCode.Conflict,
                ArgumentException _ => HttpStatusCode.BadRequest,
                ValidationException _ => HttpStatusCode.BadRequest,
                MartenCommandException martenCommandException =>
                (martenCommandException.InnerException as PostgresException)?.SqlState ==
                PostgresErrorCodes.UniqueViolation
                    ? HttpStatusCode.Conflict
                    : HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError
            };

            return new HttpStatusCodeInfo(code, (exception.InnerException as PostgresException)?.Message ?? exception.Message);
        }
    }
}
