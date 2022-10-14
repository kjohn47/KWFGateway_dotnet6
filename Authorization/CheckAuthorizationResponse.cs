namespace KWFGateway.Authorization
{
    using System.Net;

    public struct CheckAuthorizationResponse
    {
        public bool IsActive { get; set; }
        public string Token { get; set; }
        public string WWWAuthenticateCode { get; set; }
        public HttpStatusCode HttpStatus { get; set; }
    }
}
