namespace KWFGateway.Gateway
{
    using System.Net;

    public class GatewayException : Exception
    {
        public GatewayException(string message, HttpStatusCode httpStatus): base(message)
        {
            HttpStatus = httpStatus;
        }

        public HttpStatusCode HttpStatus { get; private set; }
    }
}
