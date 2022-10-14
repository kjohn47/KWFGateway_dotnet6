namespace KWFGateway.Middleware
{
    using KWFGateway.Gateway;
    using Microsoft.Extensions.Primitives;

    using System.Net;

    public class GatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IKwfHttpClientHandler _httpClientHandler;

        public GatewayMiddleware(RequestDelegate next, IKwfHttpClientHandler httpClientHandler)
        {
            _next = next;
            _httpClientHandler = httpClientHandler;
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
                await apiResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    return;
                }

                if (ex is GatewayException gwException)
                {
                    //do stuff
                    context.Response.StatusCode = (int)gwException.HttpStatus;
                    await context.Response.WriteAsync(gwException.Message);
                    return;
                }

                if (ex is TimeoutException || (ex != null && ex.InnerException is TimeoutException))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.GatewayTimeout;
                    return;
                }

                if (ex is WebException || (ex != null && ex.InnerException is WebException))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return;
                }                

                if (ex is OperationCanceledException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return;
        }

        private void CopyApiResponseHeaders(HttpContext context, HttpResponseMessage apiResponseMessage)
        {
            foreach (var header in apiResponseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in apiResponseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
    }
}
