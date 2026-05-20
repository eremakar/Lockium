using Data.Repository.Dapper;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Services
{
    public class RoleService : IRoleService
    {
        private readonly IDapperDbContext dapperDb;

        public RoleService(IDapperDbContext dapperDb)
        {
            this.dapperDb = dapperDb;
        }
        public async Task<Role> ByUserName(string userName)
        {
            return await dapperDb.Find<Role>(@"
                select r.*
                from ""UserRoles"" ur
                    join ""Users"" u on u.""Id""=ur.""UserId""
                    join ""Roles"" r on r.""Id""=ur.""RoleId""
                /**where**/
            ", where: (_) =>
            {
                _.Where(@"u.""UserName"" = @userName", new { userName });
            });

        }
    }
}
