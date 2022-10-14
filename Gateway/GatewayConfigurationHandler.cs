namespace KWFGateway.Gateway
{
    using System;
    using System.Collections.Concurrent;

    public class GatewayConfigurationHandler : IGatewayConfigurationHandler
    {
        private readonly IReadOnlyList<GatewayClientConfiguration> _clientConfigurations;
        private readonly IDictionary<string, RoundRobinPortConfig> _roundRobin;
        private readonly ConcurrentDictionary<string, object> _roundRobinLocks;

        public GatewayConfigurationHandler(IReadOnlyList<GatewayClientConfiguration> clientConfigurations)
        {
            _clientConfigurations = clientConfigurations;
            _roundRobin = new Dictionary<string, RoundRobinPortConfig>();
            _roundRobinLocks = new ConcurrentDictionary<string, object>();
            foreach(var config in clientConfigurations)
            {
                if (config.HasMultiplePorts)
                {
                    _roundRobinLocks.TryAdd(config.ServiceName, new object());
                    _roundRobin.TryAdd(config.ServiceName, new RoundRobinPortConfig
                    {
                        Current = 0,
                        Count = config.Ports.Count
                    });
                }
            }
        }

        public IReadOnlyCollection<GatewayClientConfiguration> GetConfigurations()
        {
            return _clientConfigurations;
        }

        public GatewayConfiguration? GetRouteConfiguration(string[] urlParts, string method)
        {
            // complex logic
            var serviceName = urlParts.First().ToLowerInvariant();
            //Match route base
            var config = _clientConfigurations.FirstOrDefault(x => x.ServiceName.Equals(serviceName));

            if (config != null)
            {
                //Match route parts and method
                var routeConfig = config.GetMatchRouteConfiguration(urlParts, method);
                if (routeConfig != null)
                {
                    return new GatewayConfiguration
                    {
                        Key = $"{serviceName}#{GetNextPort(config)}",
                        RouteConfiguration = routeConfig,
                        GlobalAuthorization = config.AuthorizationRequired,
                        AuthorizationHeader = config.AuthorizationHeader
                    };
                }
            }

            return null;
        }

        private int GetNextPort(GatewayClientConfiguration config)
        {
            if (!config.HasMultiplePorts)
            {
                return config.Ports[0];
            }

            var returnPort = 0;
            lock (_roundRobinLocks[config.ServiceName])
            {
                var robin = _roundRobin[config.ServiceName];
                returnPort = config.Ports[robin.Current];
                robin.Current++;
                if (robin.Current == robin.Count)
                {
                    robin.Current = 0;
                }
            }

            return returnPort;
        }
    }
}
