using Api.AspNetCore.Services;
using Lockium.Models;

namespace Lockium.Client.Api.Services
{
    public interface IMicroserviceAuthorizeService : IAuthorizeService
    {
        Task<MicroserviceAuthorizationData> AuthorizeData(Action<MicroserviceAuthorizationData> action);
    }
}
