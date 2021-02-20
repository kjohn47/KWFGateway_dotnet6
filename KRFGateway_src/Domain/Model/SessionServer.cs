namespace KRFGateway.Domain.Model
{
    public class SessionServer
    {
        public string CheckSessionUrl { get; set; }
        public int? Timeout { get; set; }
        public string CertificatePath { get; set; }
        public string CertificateKey { get; set; }
        public bool? ForceDisableSSL { get; set; }
    }
}
