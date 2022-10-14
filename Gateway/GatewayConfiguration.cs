namespace KWFGateway.Gateway
{
    public struct GatewayConfiguration
    {
        public string Key { get; set; }
        public bool GlobalAuthorization { get; set; }
        public string AuthorizationHeader { get; set; }
        public GatewayRouteConfiguration? RouteConfiguration { get; set; }
    }
}
