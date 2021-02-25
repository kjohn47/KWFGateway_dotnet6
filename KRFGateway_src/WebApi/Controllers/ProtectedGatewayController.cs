namespace KRFGateway.WebApi.Controllers
{
    using KRFGateway.App.Constants;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Authorize]
    [Route( AppConstants.ProtectedRoute )]
    public class ProtectedGatewayController : BaseGatewayController
    {
    }
}
