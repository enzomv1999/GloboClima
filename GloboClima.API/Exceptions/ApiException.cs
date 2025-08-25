using System.Net;

namespace GloboClima.API.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }

        public ApiException(
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
            string message = "An error occurred while processing your request.",
            string title = "Server Error",
            string type = "https://tools.ietf.org/html/rfc7231#section-6.6.1")
            : base(message)
        {
            StatusCode = statusCode;
            Title = title;
            Type = type;
        }
    }

    public class ValidationException : ApiException
    {
        public new IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base(HttpStatusCode.BadRequest, "One or more validation errors occurred.", "Validation Error", "https://tools.ietf.org/html/rfc7231#section-6.5.1")
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}
