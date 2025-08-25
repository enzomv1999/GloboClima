using System.Text;
using System.Text.Json;

namespace GloboClima.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = await FormatRequest(context.Request);
            _logger.LogInformation($"Request: {request}");

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                var response = await FormatResponse(context.Response);
                _logger.LogInformation($"Response: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred");
                throw;
            }
            finally
            {
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            
            var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            
            request.Body.Position = 0;
            
            return $"Method: {request.Method}, Path: {request.Path}, QueryString: {request.QueryString}, Body: {bodyAsText}";
        }

        private static async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            
            return $"Status: {response.StatusCode}, Body: {text}";
        }
    }
}
