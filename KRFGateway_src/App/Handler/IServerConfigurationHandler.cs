namespace KRFGateway.App.Handler
{
    using KRFGateway.Domain.Model;
    public interface IServerConfigurationHandler
    {
        ServerConfiguration GetServerConfiguration( string serverName );
    }
}
