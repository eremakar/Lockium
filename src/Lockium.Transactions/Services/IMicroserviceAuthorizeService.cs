using Lockium.Transactions.Models;
using Api.AspNetCore.Services;

namespace Lockium.Transactions.Services
{
    public interface IMicroserviceAuthorizeService : IAuthorizeService
    {
        Task<MicroserviceAuthorizationData> AuthorizeData(Action<MicroserviceAuthorizationData> action);
    }
}
