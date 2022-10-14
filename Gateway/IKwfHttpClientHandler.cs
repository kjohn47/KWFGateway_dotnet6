namespace KWFGateway.Gateway
{
    public interface IKwfHttpClientHandler
    {
        public Task<HttpResponseMessage> CallEndpoint(HttpContext context);
    }
}
