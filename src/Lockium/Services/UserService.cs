using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Mappings;
using Lockium.Models.Queries.Users;
using Data.Repository;
using Data.Repository.Dapper;
using Data.Repository.Helpers;
using Data.Repository.Stability;
using Microsoft.EntityFrameworkCore;

namespace Lockium.Services
{
    public class UserService : IUserService
    {
        private readonly IDapperDbContext dapperDb;
        private readonly LockiumDbContext db;
        private readonly UserMap userMap;

        public UserService(IDapperDbContext dapperDb, LockiumDbContext db, UserMap userMap)
        {
            this.dapperDb = dapperDb;
            this.db = db;
            this.userMap = userMap;
        }

        public async Task<User> FindByUserName(string userName)
        {
            return await dapperDb.FindWhereColumnValueEqualsAsync<User>("Users", "UserName", userName);
        }

        public async Task<PagedList<User>> SearchAsync(UserQuery query)
        {
            return await RetryHelper.InvokeDbAsync(async () =>
            {
                var list = await dapperDb.SearchPageAsync("Users", query);
                var result = list.Result;                

                return list;
            });
        }

        public async Task<User> FindAsync(int id)
        {
            return await RetryHelper.InvokeDbAsync(async () =>
            {
                var result = await dapperDb.FindWhereColumnValueEqualsAsync<User>("Users", "Id", id);                

                return result;
            });
        }

        public async Task<bool> AddAsync(User user, bool encryptPassword = true)
        {
            if (await db.Users.AnyAsync(_ => _.UserName == user.UserName))
                return false;

            if (encryptPassword)
                user.PasswordHash = CryptHelper.EncryptString(user.PasswordHash);

            await db.AddAsync(user);

            return true;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var original = await db.Users.FindAsync(user.Id);

            if (original == null)
                return false;

            if (!string.IsNullOrEmpty(user.PasswordHash))
                user.PasswordHash = CryptHelper.EncryptString(user.PasswordHash);

            userMap.Map(user, original);

            return true;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            var original = await db.Users.FindAsync(id);

            if (original == null)
                return false;

            db.Users.Remove(original);

            return true;
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
