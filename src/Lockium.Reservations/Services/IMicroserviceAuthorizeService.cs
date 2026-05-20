using Api.AspNetCore.Services;
using Lockium.Models;

namespace Lockium.Reservations.Services
{
    public interface IMicroserviceAuthorizeService : IAuthorizeService
    {
        Task<MicroserviceAuthorizationData> AuthorizeData(Action<MicroserviceAuthorizationData> action);
    }
}
