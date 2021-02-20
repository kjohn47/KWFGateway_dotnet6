namespace KRFGateway.App.Handler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    using KRFCommon.Constants;
    using KRFCommon.Context;
    using KRFCommon.CQRS.Common;
    using KRFCommon.JSON;

    using KRFGateway.Domain.Model;

    using Microsoft.AspNetCore.Http;

    public class RouteHandler : IRouteHandler
    {
        private readonly IServerConfigurationHandler serverConfigurationHandler;
        private readonly IHttpContextAccessor httpContext;
        private readonly IUserContext userContext;
        private readonly SessionServer sessionServer;

        public RouteHandler( IServerConfigurationHandler _serverConfigurationHandler, IHttpContextAccessor _httpContext, IUserContext _userContext, SessionServer _sessionServer )
        {
            this.serverConfigurationHandler = _serverConfigurationHandler;
            this.httpContext = _httpContext;
            this.userContext = _userContext;
            this.sessionServer = _sessionServer;
        }

        public async Task<RequestHandlerResponse> HandleRequest( HttpMethodEnum method, string serverName )
        {
            return await this.HandleRequestInternal( method, serverName );
        }

        public async Task<RequestHandlerResponse> HandleRequest( HttpMethodEnum method, string serverName, string serverAction )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction );
        }

        public async Task<RequestHandlerResponse> HandleRequest( HttpMethodEnum method, string serverName, string serverAction, string serverRoute )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction, serverRoute );
        }

        public async Task<RequestHandlerResponse> HandleRequest( HttpMethodEnum method, string serverName, string serverAction, string serverRoute, string serverRouteParam )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction, serverRoute, serverRouteParam );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( HttpMethodEnum method, string body, string serverName )
        {
            return await this.HandleRequestInternal( method, serverName, null, null, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( HttpMethodEnum method, string body, string serverName, string serverAction )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction, null, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction, serverRoute, null, body );
        }

        public async Task<RequestHandlerResponse> HandleRequestWithBody( HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute, string serverRouteParam )
        {
            return await this.HandleRequestInternal( method, serverName, serverAction, serverRoute, serverRouteParam, body );
        }

        private async Task<RequestHandlerResponse> HandleRequestInternal( HttpMethodEnum method, string serverName, string serverAction = null, string serverRoute = null, string serverRouteParam = null, string body = null )
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
                    Error = new ErrorOut( HttpStatusCode.NotFound, "Could not find requested route" )
                };
            }

            if ( ( this.userContext == null || this.userContext.Claim == Claims.NotLogged ) && routeConfig.NeedAuthorization )
            {
                this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, "Bearer Failed authentication" );
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.Unauthorized, "You need to be authenticated to access that route" )
                };
            }

            if ( routeConfig.NeedRequestBody && string.IsNullOrEmpty( body ) )
            {
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.BadRequest, "Missing Request body (JSON Format)" )
                };
            }

            if ( this.userContext != null && this.userContext.Claim != Claims.NotLogged )
            {
                //validate user session and claims, extend session
                using ( var sessionHandler = new HttpClientHandler() )
                {
                    if ( this.sessionServer.ForceDisableSSL.HasValue && this.sessionServer.ForceDisableSSL.Value )
                    {
                        sessionHandler.ServerCertificateCustomValidationCallback = ( message, cert, chain, err ) =>
                        {
                            return true;
                        };
                    }
                    else
                    {
                        if ( !string.IsNullOrEmpty( this.sessionServer.CertificatePath ) )
                        {
                            if ( !string.IsNullOrEmpty( this.sessionServer.CertificateKey ) )
                            {

                            }
                        }
                    }
                    using ( var sessionClient = new HttpClient( sessionHandler ) )
                    {
                        sessionClient.BaseAddress = null;
                        sessionClient.DefaultRequestHeaders.Accept.Append( new MediaTypeWithQualityHeaderValue( KRFConstants.JsonContentType ) );
                        if ( this.sessionServer.Timeout.HasValue )
                        {
                            sessionClient.Timeout = new TimeSpan( 0, 0, this.sessionServer.Timeout.Value );
                        }

                        using ( HttpContent sessionReq = new StringContent( JsonSerializer.Serialize( this.userContext, KRFJsonSerializerOptions.GetJsonSerializerOptions() ), Encoding.UTF8 ) )
                        {
                            sessionReq.Headers.ContentType = new MediaTypeHeaderValue( KRFConstants.JsonContentUtf8Type );
                            var sessionResponse = await sessionClient.PostAsync( this.sessionServer.CheckSessionUrl, sessionReq );

                            if ( sessionResponse == null || sessionResponse.Content == null )
                            {
                                return new RequestHandlerResponse
                                {
                                    Error = new ErrorOut( HttpStatusCode.BadRequest, "Session server didn't respond in time" )
                                };
                            }

                            var sessionInfo = await JsonSerializer.DeserializeAsync<CheckSessionResult>( await sessionResponse.Content.ReadAsStreamAsync() );
                            if( sessionInfo == null || ( sessionInfo.HasError.HasValue && sessionInfo.HasError.Value ) )
                            {
                                this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, "Bearer Failed authentication" );
                                return new RequestHandlerResponse
                                {
                                    Error = sessionInfo.Error
                                };
                            }
                        }
                    }
                }

                //generate api specific token
                token = KRFJwtConstants.Bearer + Jose.JWT.Encode( this.userContext, Encoding.UTF8.GetBytes( serverConfig.InternalTokenKey ), Jose.JwsAlgorithm.HS256 );
            }

            //Call api
            HttpResponseMessage response = null;
            using ( var handler = new HttpClientHandler() )
            {
                if ( serverConfig.ForceDisableSSL )
                {
                    handler.ServerCertificateCustomValidationCallback = ( message, cert, chain, err ) =>
                    {
                        return true;
                    };
                }
                else
                {
                    if ( !string.IsNullOrEmpty( serverConfig.CertificatePath ) )
                    {
                        if ( !string.IsNullOrEmpty( serverConfig.CertificateKey ) )
                        {

                        }
                    }
                }

                using ( var client = new HttpClient( handler ) )
                {
                    client.BaseAddress = new Uri( serverConfig.ServerUrl.EndsWith( "/" ) ? serverConfig.ServerUrl : $"{serverConfig.ServerUrl}/" );
                    if ( !string.IsNullOrEmpty( token ) )
                    {
                        client.DefaultRequestHeaders.Add( serverConfig.InternalTokenIdentifier, token );
                    }

                    if ( serverConfig.RequestTimeOut.HasValue )
                    {
                        client.Timeout = new TimeSpan( 0, 0, serverConfig.RequestTimeOut.Value );
                    }

                    client.DefaultRequestHeaders.Accept.Append( new MediaTypeWithQualityHeaderValue( "*/*" ) );

                    switch ( method )
                    {
                        case HttpMethodEnum.GET:
                        {
                            response = await client.GetAsync( string.Format( "{0}{1}", route, queryString ) );
                            break;
                        }
                        case HttpMethodEnum.DELETE:
                        {
                            response = await client.DeleteAsync( string.Format( "{0}{1}", route, queryString ) );
                            break;
                        }
                        case HttpMethodEnum.POST:
                        case HttpMethodEnum.PUT:
                        {
                            using HttpContent req = new StringContent( body ?? string.Empty, Encoding.UTF8 );
                            req.Headers.ContentType = new MediaTypeHeaderValue( KRFConstants.JsonContentUtf8Type );

                            if ( method.Equals( HttpMethodEnum.POST ) )
                            {
                                response = await client.PostAsync( string.Format( "{0}{1}", route, queryString ), req );
                            }
                            else
                            {
                                response = await client.PutAsync( string.Format( "{0}{1}", route, queryString ), req );
                            }
                            break;
                        }
                    }
                }
            }

            if ( response == null || response.Content == null )
            {
                return new RequestHandlerResponse
                {
                    Error = new ErrorOut( HttpStatusCode.BadRequest, "Server failed to retrieve response" )
                };
            }

            if ( response.Headers.WwwAuthenticate.Count > 0 )
            {
                this.httpContext.HttpContext.Response.Headers.Append( KRFConstants.AuthenticateHeader, response.Headers.WwwAuthenticate.First().ToString() );
            }

            return new RequestHandlerResponse
            {
                Response = await JsonSerializer.DeserializeAsync<object>( await response.Content.ReadAsStreamAsync() ),
                ResponseHttpStatus = ( int ) response.StatusCode
            };
        }
    }
}
