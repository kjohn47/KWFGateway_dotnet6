namespace KRFGateway.WebApi.Controllers
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    using KRFCommon.Controller;
    using KRFCommon.Proxy;

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
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.GET, serverName ) );
        }

        [HttpGet( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> GetServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction ) );
        }

        [HttpGet( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> GetServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction, serverRoute ) );
        }

        [HttpGet( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> GetServerActionRouteParam(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.GET, serverName, serverAction, serverRoute, serverRouteParam ) );
        }

        [HttpPost( "{serverName}" )]
        public async Task<IActionResult> PostServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName ) );
        }

        [HttpPost( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> PostServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction ) );
        }

        [HttpPost( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> PostServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction, serverRoute ) );
        }
        [HttpPost( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> PostServerActionRouteParam(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.POST, body?.ToString(), serverName, serverAction, serverRoute, serverRouteParam ) );
        }

        [HttpPut( "{serverName}" )]
        public async Task<IActionResult> PutServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName ) );
        }

        [HttpPut( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> PutServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction ) );
        }

        [HttpPut( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> PutServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction, serverRoute ) );
        }

        [HttpPut( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> PutServerActionRouteParam(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam,
        [FromBody] JsonElement? body )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequestWithBody( HttpMethodEnum.PUT, body?.ToString(), serverName, serverAction, serverRoute, serverRouteParam ) );
        }

        [HttpDelete( "{serverName}" )]
        public async Task<IActionResult> DeleteServer(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName ) );
        }

        [HttpDelete( "{serverName}/{serverAction}" )]
        public async Task<IActionResult> DeleteServerAction(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction ) );
        }

        [HttpDelete( "{serverName}/{serverAction}/{serverRoute}" )]
        public async Task<IActionResult> DeleteServerActionRoute(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction, serverRoute ) );
        }

        [HttpDelete( "{serverName}/{serverAction}/{serverRoute}/{serverRouteParam}" )]
        public async Task<IActionResult> DeleteServerActionRouteParam(
        [FromServices] IRouteHandler routeHandler,
        [FromRoute] string serverName,
        [FromRoute] string serverAction,
        [FromRoute] string serverRoute,
        [FromRoute] string serverRouteParam )
        {
            return await this.ProcessRequestAsync( () => routeHandler.HandleRequest( HttpMethodEnum.DELETE, serverName, serverAction, serverRoute, serverRouteParam ) );
        }

        private async Task<ObjectResult> ProcessRequestAsync(Func<Task<RequestHandlerResponse>> request)
        {
            var response = await request();

            if ( response.HasError )
            {
                return this.StatusCode( response.GetStatusCode, response.Error );
            }

            return this.StatusCode( response.GetStatusCode, response.Response );
        }
    }
}
