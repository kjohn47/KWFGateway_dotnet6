using KRFCommon.CQRS.Common;

namespace KRFGateway.Domain.Model
{
    public class RequestHandlerResponse
    {
        public object Response { get; set; }

        public int ResponseHttpStatus { get; set; }

        public ErrorOut Error { get; set; }
    }
}
