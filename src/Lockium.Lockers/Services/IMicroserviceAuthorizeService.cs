using Lockium.Lockers.Models;
using Api.AspNetCore.Services;

namespace Lockium.Lockers.Services
{
    public interface IMicroserviceAuthorizeService : IAuthorizeService
    {
        Task<MicroserviceAuthorizationData> AuthorizeData(Action<MicroserviceAuthorizationData> action);
    }
}
