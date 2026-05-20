using Lockium.Data.LockiumDb.Entities;
using Lockium.Models.Queries.Users;
using Data.Repository;

namespace Lockium.Services
{
    public interface IUserService
    {
        Task<User> FindByUserName(string userName);
        Task<PagedList<User>> SearchAsync(UserQuery query);
        Task<User> FindAsync(int id);
        Task<bool> AddAsync(User user, bool encryptPassword = true);
        Task<bool> UpdateAsync(User user);
        Task<bool> RemoveAsync(int id);

        Task SaveChangesAsync();
    }
}
