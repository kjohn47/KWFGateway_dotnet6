using System.Collections.Generic;

using KRFCommon.Proxy;

namespace KRFGateway.Domain.Model
{
    public class RouteConfiguration
    {
        private bool needRequestBody;
        public string Route { get; set; }
        public HttpMethodEnum Method { get; set; }
        public bool NeedAuthorization { get; set; }
        public IEnumerable<string> Exclude { get; set; }

        public bool NeedRequestBody
        {
            get
            {
                return this.Method.Equals( HttpMethodEnum.POST ) || this.Method.Equals( HttpMethodEnum.PUT )
                    ? this.needRequestBody
                    : false;
            }

            set
            {
                this.needRequestBody = value;
            }
        }
    }
}
