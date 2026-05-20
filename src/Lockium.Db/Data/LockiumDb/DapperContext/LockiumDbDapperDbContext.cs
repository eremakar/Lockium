using Data.Repository.Dapper;
using Microsoft.Extensions.Configuration;

namespace Lockium.Data.LockiumDb.DapperContext
{
    public partial class LockiumDbDapperDbContext : DapperDbContext
    {
        public LockiumDbDapperDbContext(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
