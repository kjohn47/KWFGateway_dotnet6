namespace KWFGateway.Middleware
{
    using KWFGateway.Common;
    using Microsoft.Extensions.Configuration;

    using System.Net;

    public class FaviconMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FaviconConfiguration? _configuration;
        private readonly object _lockObject = new();
        private byte[]? _faviconBytes = null;

        public FaviconMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration.GetSection(nameof(FaviconConfiguration)).Get<FaviconConfiguration>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.Equals("/favicon.ico") && !context.Response.HasStarted)
            {
                if (_configuration is null || !_configuration.UseFavicon)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.BodyWriter.WriteAsync(null);
                    return;
                }

                if (_faviconBytes == null)
                {
                    lock (_lockObject)
                    {
                        if (_faviconBytes == null)
                        {
                            var faviconDirPath = Path.IsPathRooted(_configuration.FaviconPath) && !_configuration.FaviconUseCurrentDomainPath
                                             ? Path.GetFullPath(_configuration.FaviconPath)
                                             : _configuration.FaviconUseCurrentDomainPath
                                               ? string.IsNullOrEmpty(_configuration.FaviconPath) ? AppDomain.CurrentDomain.BaseDirectory : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configuration.FaviconPath)
                                               : string.IsNullOrEmpty(_configuration.FaviconPath) ? Environment.CurrentDirectory : Path.GetFullPath(_configuration.FaviconPath, Environment.CurrentDirectory);

                            if (!Directory.Exists(faviconDirPath))
                            {
                                throw new DirectoryNotFoundException($"{faviconDirPath} could not be found");
                            }

                            var faviconFilePath = Path.Combine(faviconDirPath, "favicon.ico");

                            if (!File.Exists(faviconFilePath))
                            {
                                throw new FileNotFoundException($"{faviconFilePath} could not be found");
                            }

                            //read favicon file bytes
                            _faviconBytes = File.ReadAllBytes(faviconFilePath);
                        }
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Headers.ContentType = "image/x-icon";
                context.Response.Headers.Add("Content-Security-Policy", "default-src *; img-src * 'self' data: https: http:; script-src 'self' 'unsafe-inline' 'unsafe-eval' *;style-src 'self' 'unsafe-inline' * ");
                context.Response.ContentLength = _faviconBytes.Length;
                await context.Response.StartAsync(context.RequestAborted);
                await context.Response.BodyWriter.WriteAsync(_faviconBytes, context.RequestAborted);
                await context.Response.CompleteAsync();
                return;
            }

            await _next(context);
        }
    }
}
