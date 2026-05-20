using Api.AspNetCore.Services;

namespace Lockium.Devices.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
