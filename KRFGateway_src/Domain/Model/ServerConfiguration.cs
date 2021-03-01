using System.Collections.Generic;

namespace KRFGateway.Domain.Model
{
    public class ServerConfiguration
    {
        public string ServerName { get; set; }
        public string ServerUrl { get; set; }
        public string InternalTokenKey { get; set; }
        public string InternalTokenIdentifier { get; set; }
        public bool CopyJWTFromRequestOnOpenRoute { get; set; }
        public bool ForceDisableSSL { get; set; }
        public string CertificatePath { get; set; }
        public string CertificateKey { get; set; }
        public int? RequestTimeOut { get; set; }
        public int? TokenExpirationMinutes { get; set; }
        public IEnumerable<RouteConfiguration> RouteConfiguration { get; set; }
    }
}
