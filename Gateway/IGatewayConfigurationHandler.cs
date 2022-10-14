namespace KWFGateway.Gateway
{
    public interface IGatewayConfigurationHandler
    {
        public GatewayConfiguration? GetRouteConfiguration(string[] urlParts, string method);
    }
}
