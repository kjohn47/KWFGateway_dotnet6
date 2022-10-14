namespace KWFGateway.Common
{
    using System.Security.Authentication;

    public class ApiCertificateConfiguration
    {
        public GatewaySSLCertificateEnum CertificateSource { get; set; } = GatewaySSLCertificateEnum.NotSet;
        public string CertificateValue { get; set; } = string.Empty;
        public string? CertificatePrivateValue { get; set; } = null;
        public string? CertificatePassword { get; set; } = null;
        public SslProtocols SslProtocols { get; set; } = SslProtocols.None;
    }
}
