using Api.AspNetCore.Services;

namespace Lockium.Orders.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
