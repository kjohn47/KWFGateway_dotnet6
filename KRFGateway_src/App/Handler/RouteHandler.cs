namespace KRFGateway.App.Handler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using KRFCommon.Constants;
    using KRFCommon.Context;
    using KRFCommon.CQRS.Common;
    using KRFCommon.Proxy;

    using KRFGateway.Domain.Model;

    using Microsoft.AspNetCore.Http;

    public class RouteHandler : IRouteHandler
    {
        private readonly IServerConfigurationHandler serverConfigurationHandler;
        private readonly IHttpContextAccessor httpContext;
        private readonly IUserContext userContext;
        private readonly GatewaySettings gatewaySettings;

        public RouteHandler( IServerConfigurationHandler _serverConfigurationHandler, IHttpContextAccessor _httpContext, IUserContext _userContext, GatewaySettings _gatewaySettings )
        {
            this.serverConfigurationHandler = _serverConfigurationHandler;
            this.httpContext = _httpContext;
            this.userContext = _userContext;
            this.gatewaySettings = _gatewaySettings;
        }

        public async Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName )
        {
            return await this.HandleRequestInternal( open, method, serverName );
        }

        public async Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction );
        }

        public async Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction, string serverRoute )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction, serverRoute );
        }

        public async Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction, string serverRoute, string serverRouteParam )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction, serverRoute, serverRouteParam );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName )
        {
            return await this.HandleRequestInternal( open, method, serverName, null, null, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction, null, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction, serverRoute, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute, string serverRouteParam )
        {
            return await this.HandleRequestInternal( open, method, serverName, serverAction, serverRoute, serverRouteParam, body );
        }

        private async Task<RequestHandlerResponse> HandleRequestInternal( bool open, HttpMethodEnum method, string serverName, string serverAction = null, string serverRoute = null, string serverRouteParam = null, string body = null )
        {
            var queryString = this.httpContext.HttpContext.Request.QueryString.HasValue ? this.httpContext.HttpContext.Request.QueryString.Value : string.Empty;
            var serverConfig = this.serverConfigurationHandler.GetServerConfiguration( serverName );
            string token = null;
            var route = $"{serverName}";
            var urlParts = new List<string> { serverName };

            if ( !string.IsNullOrEmpty( serverAction ) )
            {
                route = $"{route}/{serverAction}";
                urlParts.Add( serverAction );
            }

            if ( !string.IsNullOrEmpty( serverRoute ) )
            {
                route = $"{route}/{serverRoute}";
                urlParts.Add( serverRoute );
            }

            if ( !string.IsNullOrEmpty( serverRouteParam ) )
            {
                route = $"{route}/{serverRouteParam}";
                urlParts.Add( serverRouteParam );
            }

            var routeConfig = serverConfig.RouteConfiguration.FirstOrDefault( x =>
            {
                if ( x.NeedAuthorization && open )
                {
                    return false;
                }

                if ( x.Method.Equals( method ) )
                {
                    var configRoute = x.Route.StartsWith( "/" ) ? x.Route.Substring( 1 ) : x.Route;
                    if ( configRoute.Equals( route, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        return true;
                    }
                    else
                    {
                        var routeParts = configRoute.Split( '/' );
                        if ( routeParts.Length == urlParts.Count )
                        {
                            for ( int i = 0; i < routeParts.Length; i++ )
                            {
                                if ( !( ( routeParts[ i ].Equals( "*" ) && ( x.Exclude == null || !x.Exclude.Any( e => e.Equals( urlParts[ i ], StringComparison.InvariantCultureIgnoreCase ) ) ) ) ||
                                routeParts[ i ].Equals( urlParts[ i ], StringComparison.InvariantCultureIgnoreCase ) ) )
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
                return false;
            } );

            if ( routeConfig == null )
            {
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.NotFound, "Could not find requested route", ResponseErrorType.Application )
                };
            }

            if ( routeConfig.NeedAuthorization && ( open || this.userContext == null || this.userContext.Claim == Claims.NotLogged ) )
            {
                this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, "Bearer Failed authentication" );
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.Unauthorized, "You need to be authenticated to access that route", ResponseErrorType.Application )
                };
            }

            if ( routeConfig.NeedRequestBody && string.IsNullOrEmpty( body ) )
            {
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.BadRequest, "Missing Request body (JSON Format)", ResponseErrorType.Proxy )
                };
            }

            if ( routeConfig.NeedAuthorization )
            {
                //validate user session and claims, extend session
                if ( this.gatewaySettings.SessionServerEnabled && this.gatewaySettings.SessionServerSettings != null )
                {
                    var sessionResponse = await KRFRestHandler.RequestHttp<IUserContext, CheckSessionResult>( new KRFHttpRequestWithBody<IUserContext>
                    {
                        Url = this.gatewaySettings.SessionServerSettings.CheckSessionUrl,
                        CertificateKey = this.gatewaySettings.SessionServerSettings.CertificateKey,
                        CertificatePath = this.gatewaySettings.SessionServerSettings.CertificatePath,
                        ForceDisableSSL = this.gatewaySettings.SessionServerSettings.ForceDisableSSL,
                        Method = HttpMethodEnum.POST,
                        Timeout = this.gatewaySettings.SessionServerSettings.Timeout,
                        Body = this.userContext
                    } );

                    if ( sessionResponse.HasError || !sessionResponse.Response.Success )
                    {
                        this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, "Bearer Failed authentication" );
                        return new RequestHandlerResponse
                        {
                            Error = sessionResponse.HasError ? sessionResponse.Error : sessionResponse.Error,
                            HttpStatusCode = sessionResponse.HttpStatus
                        };
                    }
                }

                //use same token
                if ( this.gatewaySettings.AppConfiguration.TokenKey.Equals( serverConfig.InternalTokenKey ) )
                {
                    token = this.httpContext.HttpContext.Request.Headers[ this.gatewaySettings.AppConfiguration.TokenIdentifier ].ElementAt( 0 );
                }
                else
                {
                    //generate api specific token and copy expiration if no override setting is available
                    token = KRFJwt.GetSignedBearerTokenFromContext(
                        this.userContext,
                        serverConfig.InternalTokenKey,
                        serverConfig.TokenExpirationMinutes.HasValue
                            ? DateTime.Now.AddMinutes( serverConfig.TokenExpirationMinutes.Value )
                            : this.userContext.TokenExpiration );
                }
            }
            else if ( open && serverConfig.CopyJWTFromRequestOnOpenRoute &&
                this.gatewaySettings.AppConfiguration.TokenKey.Equals( serverConfig.InternalTokenKey ) &&
                this.httpContext.HttpContext.Request.Headers.ContainsKey( this.gatewaySettings.AppConfiguration.TokenIdentifier ) )
            {
                token = this.httpContext.HttpContext.Request.Headers[ this.gatewaySettings.AppConfiguration.TokenIdentifier ].ElementAt( 0 );
            }

            //Call api
            var response = await KRFRestHandler.RequestHttp<string, object>( new KRFHttpRequestWithBody<string>
            {
                Url = serverConfig.ServerUrl,
                CertificateKey = serverConfig.CertificateKey,
                CertificatePath = serverConfig.CertificatePath,
                ForceDisableSSL = serverConfig.ForceDisableSSL,
                BearerToken = token,
                BearerTokenHeader = serverConfig.InternalTokenIdentifier,
                Method = method,
                QueryString = queryString,
                Route = route,
                Timeout = serverConfig.RequestTimeOut,
                Body = body
            } );

            if ( response?.ResponseHeaders?.WwwAuthenticate?.Count > 0 )
            {
                this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, response.ResponseHeaders.WwwAuthenticate.First().ToString() );
            }

            return new RequestHandlerResponse
            {
                Response = response.Response,
                HttpStatusCode = response.HttpStatus,
                Error = response.Error
            };
        }
    }
}
