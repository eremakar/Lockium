using Lockium.Transactions.Models;
using Api.AspNetCore.Models.Scope;
using Api.AspNetCore.Services;

namespace Lockium.Transactions.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
