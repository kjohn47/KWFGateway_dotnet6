namespace KWFGateway.Authorization
{
    using KWFGateway.Common;

    public class AuthorizationConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ValidateSessionEndpoint { get; set; } = string.Empty;
        public string AuthorizationHeader { get; set; } = string.Empty;
        public string LoginRequiredCode { get; set; } = "LoginRequired";
        public bool RedirectTokenFromRequest { get; set; } = false;
        public int? Timeout { get; set; }
        public IList<int> Ports { get; set; } = Array.Empty<int>();
        public ApiCertificateConfiguration? CertificateConfiguration { get; set; }
    }
}
