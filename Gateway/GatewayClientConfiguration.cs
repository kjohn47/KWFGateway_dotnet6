namespace KWFGateway.Gateway
{
    using KWFGateway.Common;

    using System.Security.Authentication;

    public class GatewayClientConfiguration
    {
        public string ServiceName { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;
        public IList<int> Ports { get; set; } = Array.Empty<int>();
        public bool AuthorizationRequired { get; set; }
        public ApiCertificateConfiguration? CertificateConfiguration { get; set; }
        public string AuthorizationHeader { get; set; } = string.Empty;
        public int? Timeout { get; set; }
        public IList<GatewayRouteConfiguration> RouteConfiguration { get; set; } = Array.Empty<GatewayRouteConfiguration>();
        public bool HasMultiplePorts => Ports.Count > 1;

        public GatewayRouteConfiguration? GetMatchRouteConfiguration(string[] urlParts, string method)
        {
            for (int i = 0; i < RouteConfiguration.Count; i++)
            {
                var route = RouteConfiguration.ElementAt(i);
                if (route.IsMatch(urlParts, method))
                {
                    return route;
                }
            }

            return null;
        }
    }
}
