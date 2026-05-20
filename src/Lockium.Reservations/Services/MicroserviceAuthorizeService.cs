using Api.AspNetCore.Services;

namespace Lockium.Reservations.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
