namespace KRFGateway.WebApi.Controllers
{
    using System.Text.Json;
    using System.Threading.Tasks;

    using KRFCommon.Controller;

    using KRFGateway.App.Constants;
    using KRFGateway.App.Handler;
    using KRFGateway.Domain.Model;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route( AppConstants.RouteVersion )]
    public class GatewayController : KRFController
    {
        [HttpGet( "{serverName}" )]
        public async Task<IActionResult> GetServer(
            [FromServices] IRouteHandler routeHandler,
            [FromRoute] string serverName )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.GET, serverName );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpGet( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> GetServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpGet( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> GetServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction, serverRoute );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpGet( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> GetServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction, serverRoute, serverRouteParam );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPost( "{serverName}" )]
        public async Task<IActionResult> PostServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPost( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> PostServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPost( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> PostServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction, serverRoute );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }
        [HttpPost( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> PostServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction, serverRoute, serverRouteParam );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPut( "{serverName}" )]
        public async Task<IActionResult> PutServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPut( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> PutServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPut( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> PutServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction, serverRoute );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpPut( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> PutServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam,
        [FromBody] JsonElement? body )
        {
            var response = await routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction, serverRoute, serverRouteParam );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpDelete( "{serverName}" )]
        public async Task<IActionResult> DeleteServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpDelete( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> DeleteServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpDelete( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> DeleteServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction, serverRoute );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }

        [HttpDelete( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> DeleteServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam )
        {
            var response = await routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction, serverRoute, serverRouteParam );
            if ( response.Error != null )
            {
                return this.StatusCode( response.Error.ErrorStatusCode, response.Error );
            }

            return this.StatusCode( response.ResponseHttpStatus, response.Response );
        }
    }
}
