namespace KWFGateway.Gateway
{
    using KWFGateway.Authorization;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;

    public class KwfHttpClientHandler : IKwfHttpClientHandler
    {
        private readonly IGatewayConfigurationHandler _gatewayConfigurationHandler;
        private readonly IAuthorizationHandler _authorizationHandler;
        private readonly IHttpClientFactory _HttpClientFactory;
        private readonly ILogger<KwfHttpClientHandler> _logger;
        private readonly bool _logsEnabled;

        public KwfHttpClientHandler(
            IHttpClientFactory httpClientFactory,
            IGatewayConfigurationHandler gatewayConfigurationHandler, 
            IAuthorizationHandler authorizationHandler,
            ILogger<KwfHttpClientHandler> logger,
            IConfiguration configuration)
        {
            _gatewayConfigurationHandler = gatewayConfigurationHandler;
            _authorizationHandler = authorizationHandler;
            _HttpClientFactory = httpClientFactory;
            _logger = logger;
            _logsEnabled = configuration.GetValue<bool>("EnableHttpClientLogs");
        }

        public async Task<HttpResponseMessage> CallEndpoint(HttpContext context)
        {
            if (!context.Request.Path.HasValue)
            {
                throw new GatewayException("Invalid or empty endpoint", HttpStatusCode.InternalServerError);
            }

            var urlParts = context.Request.Path.Value.Split(Constants.UrlSplitChar, StringSplitOptions.RemoveEmptyEntries);
            var config = _gatewayConfigurationHandler.GetRouteConfiguration(urlParts, context.Request.Method);

            if (!config.HasValue || config.Value.RouteConfiguration == null)
            {
                throw new GatewayException("Endpoint not available or configured", HttpStatusCode.NotFound);
            }

            // get auth client if required
            // check auth
            var authorizationRequired = (!config.Value.RouteConfiguration.RequireAuthorization.HasValue && config.Value.GlobalAuthorization) || (config.Value.RouteConfiguration.RequireAuthorization ?? false);
            var hasAuthorizationHeader = !string.IsNullOrEmpty(config.Value.AuthorizationHeader);
            var bearerToken = string.Empty;
            if (authorizationRequired)
            {
                var authorizationResponse = await _authorizationHandler.CheckAuthorization(context);
                if (!authorizationResponse.IsActive)
                {
                    var response = new HttpResponseMessage
                    {
                        StatusCode = authorizationResponse.HttpStatus
                    };

                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(authorizationResponse.WWWAuthenticateCode));
                    return response;
                }

                bearerToken = authorizationResponse.Token;
            }

            var client = GetClientByKey(config.Value.Key);
            var method = GetHttpMethod(context.Request.Method);

            using var requestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = GetRequestUrl(urlParts, context.Request.QueryString)
            };

            await PopulateAndLogRequestBody(requestMessage, method, context, client.BaseAddress);
            PopulateRequestHeaders(requestMessage, context, authorizationRequired, hasAuthorizationHeader, config.Value.AuthorizationHeader, bearerToken);

            var clientResponse = await client.SendAsync(requestMessage, context.RequestAborted);

            if (_logsEnabled)
            {
                var responseBody = string.Empty;
                var mediaType = clientResponse.Content?.Headers?.ContentType?.MediaType;

                if (clientResponse.Content != null && (mediaType == MediaTypeNames.Text.Plain || mediaType == MediaTypeNames.Text.RichText || mediaType == MediaTypeNames.Application.Json))
                {
                    responseBody = await clientResponse.Content.ReadAsStringAsync();
                }

                _logger.LogInformation(
                        "Response:\nMethod:{method}\nURL: {endpoint}\nStatusCode: {httpStatusCode}\nBody: {responseBody}",
                        clientResponse.RequestMessage?.Method?.Method,
                        clientResponse.RequestMessage?.RequestUri?.ToString(),
                        clientResponse.StatusCode,
                        responseBody);
            }

            return clientResponse;
        }

        private HttpClient GetClientByKey(string key)
        {
            return _HttpClientFactory.CreateClient(key);
        }

        private async Task PopulateAndLogRequestBody(HttpRequestMessage requestMessage, HttpMethod method, HttpContext context, Uri? clientUri)
        {
            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                if (context.Request.HasFormContentType)
                {
                    await context.Request.ReadFormAsync(context.RequestAborted);
                    if (context.Request.ContentType == Constants.UrlEncodedFormData)
                    {
                        requestMessage.Content = new FormUrlEncodedContent(context.Request.Form.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
                    }
                    else
                    {
                        var content = new MultipartFormDataContent(context.Request.GetMultipartBoundary());
                        if (context.Request.Form.Files != null && context.Request.Form.Files.Count > 0)
                        {
                            for (int i = 0; i < context.Request.Form.Files.Count; i++)
                            {
                                var file = context.Request.Form.Files.ElementAt(i);
                                var fileStream = new StreamContent(file.OpenReadStream());
                                fileStream.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType.ToString());
                                content.Add(fileStream, file.Name, file.FileName);
                            }
                        }

                        for (int i = 0; i < context.Request.Form.Count; i++)
                        {
                            var formParam = context.Request.Form.ElementAt(i);
                            content.Add(new StringContent(formParam.Value), formParam.Key);
                        }

                        requestMessage.Content = content;
                    }

                    if (_logsEnabled)
                    {
                        _logger.LogInformation(
                            "Request:\nMethod: {method}\nURL: {client}/{endpoint}\nContent-type: {contentType}\nForm params: {formParams}",
                            method.Method,
                            clientUri?.ToString(),
                            requestMessage.RequestUri?.ToString(),
                            requestMessage.Content.Headers?.ContentType?.MediaType,
                            JsonSerializer.Serialize(context.Request.Form));
                    }
                }
                else
                {
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                    var reqBody = await reader.ReadToEndAsync();
                    requestMessage.Content = new StringContent(reqBody, Encoding.UTF8, context.Request.ContentType ?? MediaTypeNames.Text.Plain);

                    if (_logsEnabled)
                    {
                        _logger.LogInformation(
                            "Request:\nMethod: {method}\nURL: {client}/{endpoint}\nContent-type: {contentType}\nRequest Body: {requestBody}",
                            method.Method,
                            clientUri?.ToString(),
                            requestMessage.RequestUri?.ToString(),
                            requestMessage.Content.Headers?.ContentType?.MediaType,
                            reqBody);
                    }
                }

                return;
            }

            if (_logsEnabled)
            {
                _logger.LogInformation(
                    "Request:\nMethod: {method}\nURL: {client}/{endpoint}",
                    method.Method,
                    clientUri?.ToString(),
                    requestMessage.RequestUri?.ToString());
            }
        }

        private static void PopulateRequestHeaders(
            HttpRequestMessage requestMessage,
            HttpContext context, 
            bool authorizationRequired,
            bool hasAuthorizationHeader, 
            string authorizationHeader,
            string bearerToken)
        {
            if (authorizationRequired)
            {
                if (hasAuthorizationHeader)
                {
                    requestMessage.Headers.TryAddWithoutValidation(authorizationHeader, $"Bearer {bearerToken}");
                }
                else
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                }
            }

            for (int i = 0; i < context.Request.Headers.Count; i++)
            {
                var header = context.Request.Headers.ElementAt(i);

                if (header.Key.Equals(Microsoft.Net.Http.Headers.HeaderNames.Authorization) || (hasAuthorizationHeader && header.Key.Equals(authorizationHeader)))
                {
                    continue;
                }

                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private static Uri GetRequestUrl(string[] urlParts, QueryString queryString)
        {
            var urlBuilder = new StringBuilder();

            if (urlParts.Length > 1)
            {
                urlBuilder.Append(string.Join(Constants.UrlSplitChar, urlParts[1..]));
            }

            if (queryString.HasValue)
            {
                urlBuilder.Append(queryString.Value);
            }

            return Uri.TryCreate(urlBuilder.ToString(), UriKind.Relative, out var uri)
                   ? uri
                   : throw new GatewayException("Could not create destination url", HttpStatusCode.InternalServerError);
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            switch (method.ToUpperInvariant())
            {
                case "GET": return HttpMethod.Get;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "OPTIONS": return HttpMethod.Options;
                case "TRACE": return HttpMethod.Trace;
                case "PATCH": return HttpMethod.Patch;
                default:
                    break;
            }

            return HttpMethod.Get;
        }
    }
}
