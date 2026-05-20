using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.Users
{
    public partial class UserQuery : QueryBase<User, UserFilter, UserSort>
    {
    }
}
