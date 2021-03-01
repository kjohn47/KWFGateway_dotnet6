namespace KRFGateway.App.Handler
{
    using System.Threading.Tasks;

    using KRFCommon.Proxy;

    using KRFGateway.Domain.Model;
    public interface IRouteHandler
    {
        Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName );

        Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction );

        Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction, string serverRoute );

        Task<RequestHandlerResponse> HandleRequest( bool open, HttpMethodEnum method, string serverName, string serverAction, string serverRoute, string serverRouteParam );

        Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName );

        Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction );

        Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute );

        Task<RequestHandlerResponse> HandleRequestWithBody( bool open, HttpMethodEnum method, string body, string serverName, string serverAction, string serverRoute, string serverRouteParam );
    }
}
