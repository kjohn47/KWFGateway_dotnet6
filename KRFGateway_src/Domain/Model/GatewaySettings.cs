namespace KRFGateway.Domain.Model
{
    using KRFCommon.Api;
    public class GatewaySettings
    {
        public bool SessionServerEnabled { get; set; }
        public SessionServer SessionServerSettings { get; set; }
        public AppConfiguration AppConfiguration { get; set; }
    }
}
