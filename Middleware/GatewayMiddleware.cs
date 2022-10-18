namespace KWFGateway.Middleware
{
    using KWFGateway.Gateway;

    using Microsoft.AspNetCore.Http;

    using System.Net;

    public class GatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IKwfHttpClientHandler _httpClientHandler;
        private readonly ILogger<GatewayMiddleware> _logger;
        private readonly bool _logsEnabled;

        public GatewayMiddleware(RequestDelegate next, IKwfHttpClientHandler httpClientHandler, ILogger<GatewayMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _httpClientHandler = httpClientHandler;
            _logger = logger;
            _logsEnabled = configuration.GetValue<bool>("EnableGatewayLogs");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (context.Response.HasStarted)
                {
                    return;
                }

                var apiResponse = await _httpClientHandler.CallEndpoint(context);
                context.Response.StatusCode = (int)apiResponse.StatusCode;
                CopyApiResponseHeaders(context, apiResponse);
                await context.Response.StartAsync(context.RequestAborted);
                await apiResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
                await context.Response.CompleteAsync();
                return;
            }
            catch (Exception ex)
            {
                if (ex is GatewayException gwException)
                {
                    await ReturnResponse(context, gwException.HttpStatus, ex, gwException.Message);
                    return;
                }

                if (ex is HttpRequestException)
                {
                    await ReturnResponse(context, HttpStatusCode.ServiceUnavailable, ex);
                    return;
                }

                if (ex is TimeoutException || (ex != null && ex.InnerException is TimeoutException))
                {
                    await ReturnResponse(context, HttpStatusCode.GatewayTimeout, ex);
                    return;
                }

                if (ex is WebException || (ex != null && ex.InnerException is WebException))
                {
                    await ReturnResponse(context, HttpStatusCode.InternalServerError, ex);
                    return;
                }

                if (ex is OperationCanceledException)
                {
                    await ReturnResponse(context, HttpStatusCode.Accepted, ex);
                    return;
                }

                await ReturnResponse(context, HttpStatusCode.InternalServerError, ex);
                return;
            }
        }

        private async Task ReturnResponse(HttpContext context, HttpStatusCode httpStatusCode, Exception? ex, string? message = null)
        {
            if (_logsEnabled)
            {
                _logger.LogError(ex, "Exception occurred: {exceptionMessage}", message ?? string.Empty);
            }

            context.Response.StatusCode = (int)httpStatusCode;

            if (!string.IsNullOrEmpty(message))
            {
                await context.Response.StartAsync(context.RequestAborted);
                await context.Response.WriteAsync(message);
                await context.Response.CompleteAsync();
                return;
            }

            await context.Response.StartAsync(context.RequestAborted);
            await context.Response.BodyWriter.WriteAsync(null);
            await context.Response.CompleteAsync();
        }

        private static void CopyApiResponseHeaders(HttpContext context, HttpResponseMessage apiResponseMessage)
        {
            foreach (var header in apiResponseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in apiResponseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            //TODO;
            context.Response.Headers.Add(Constants.CspHeader);
            context.Response.Headers.Remove("transfer-encoding");
        }
    }
}
