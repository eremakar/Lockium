using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.UserRoles
{
    public partial class UserRoleFilter : FilterBase<UserRole>
    {
        public FilterOperand<int>? Id { get; set; }
        public FilterOperand<int?>? UserId { get; set; }
        public FilterOperand<int?>? RoleId { get; set; }
    }
}
