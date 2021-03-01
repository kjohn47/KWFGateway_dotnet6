namespace KRFGateway.WebApi.Controllers
{
    using KRFGateway.App.Constants;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route( AppConstants.OpenRoute )]
    public class OpenGatewayController : BaseGatewayController
    {
        public OpenGatewayController() : base( true )
        {
        }
    }
}
