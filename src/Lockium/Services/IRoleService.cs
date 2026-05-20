using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Services
{
    public interface IRoleService
    {
        Task<Role> ByUserName(string userName);
    }
}
