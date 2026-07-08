using Lockium.Billings.Models;
using Api.AspNetCore.Services;

namespace Lockium.Billings.Services
{
    public interface IMicroserviceAuthorizeService : IAuthorizeService
    {
        Task<MicroserviceAuthorizationData> AuthorizeData(Action<MicroserviceAuthorizationData> action);
    }
}
