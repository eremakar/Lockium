using Lockium.Billings.Models;
using Api.AspNetCore.Models.Scope;
using Api.AspNetCore.Services;

namespace Lockium.Billings.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
