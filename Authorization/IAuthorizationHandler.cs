namespace KWFGateway.Authorization
{
    public interface IAuthorizationHandler
    {
        public Task<CheckAuthorizationResponse> CheckAuthorization(HttpContext context);
    }
}
