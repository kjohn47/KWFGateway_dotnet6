namespace KWFGateway.Authorization
{
    using KWFGateway.Gateway;

    using System.Net;
    using System.Net.Http.Headers;

    public class AuthorizationHandler : IAuthorizationHandler
    {
        public const string AuthorizationApiKey = "AuthorizationApi";
        private readonly RoundRobinPortConfig _roundRobin;
        private readonly object _roundRobinLock;
        private readonly AuthorizationConfig _config;
        private readonly bool _hasMultiplePorts = false;
        private readonly bool _hasAuthorizationHeaderSpecified = false;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthorizationHandler(AuthorizationConfig configuration, IHttpClientFactory httpClientFactory)
        {
            _roundRobin = new RoundRobinPortConfig();
            _roundRobinLock = new object();
            _config = configuration;
            _hasMultiplePorts = _config.Ports.Count > 1;
            _hasAuthorizationHeaderSpecified = !string.IsNullOrEmpty(_config.AuthorizationHeader);
            _httpClientFactory = httpClientFactory;
        }

        public async Task<CheckAuthorizationResponse> CheckAuthorization(HttpContext context)
        {
            var headerToken = _hasAuthorizationHeaderSpecified 
                        ? context.Request.Headers.TryGetValue(_config.AuthorizationHeader, out var tokenValue) 
                          ? tokenValue 
                          : string.Empty
                        : context.Request.Headers.Authorization;

            

            if (!AuthenticationHeaderValue.TryParse(headerToken, out var token) || token?.Parameter == null)
            {
                return new CheckAuthorizationResponse
                {
                    IsActive = false,
                    WWWAuthenticateCode = _config.LoginRequiredCode,
                    HttpStatus = HttpStatusCode.Unauthorized
                };
            }

            if (_config.RedirectTokenFromRequest)
            {
                return new CheckAuthorizationResponse
                {
                    IsActive = true,
                    Token = token.Parameter
                };
            }

            var client = _httpClientFactory.CreateClient($"{AuthorizationApiKey}#{GetNextPort()}");
            await Task.Yield();
            return new CheckAuthorizationResponse
            {
                IsActive = false,
                WWWAuthenticateCode = "ErrorFromService",
                HttpStatus = HttpStatusCode.Unauthorized //From service
            };
        }

        private int GetNextPort()
        {
            if (!_hasMultiplePorts)
            {
                return _config.Ports[0];
            }

            var returnPort = 0;
            lock (_roundRobinLock)
            {
                returnPort = _config.Ports[_roundRobin.Current];
                _roundRobin.Current++;
                if (_roundRobin.Current == _roundRobin.Count)
                {
                    _roundRobin.Current = 0;
                }
            }

            return returnPort;
        }
    }
}
