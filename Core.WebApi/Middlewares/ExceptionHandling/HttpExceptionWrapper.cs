namespace Shipments.Api.Exceptions
{
    public class HttpExceptionWrapper
    {
        public int StatusCode { get; }

        public string Error { get; }

        public HttpExceptionWrapper(int statusCode, string error)
        {
            StatusCode = statusCode;
            Error = error;
        }
    }
}
