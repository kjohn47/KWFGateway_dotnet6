using System.Net;

using KRFCommon.CQRS.Common;

namespace KRFGateway.Domain.Model
{
    public class RequestHandlerResponse
    {

        public object Response { get; set; }

        public HttpStatusCode? HttpStatusCode { get; set; }

        public ErrorOut Error { get; set; }

        public bool HasError => this.Error != null;

        public int GetStatusCode => this.HttpStatusCode.HasValue ? ( int ) this.HttpStatusCode.Value : this.HasError ? ( int ) this.Error.ErrorStatusCode : 200;
    }
}
