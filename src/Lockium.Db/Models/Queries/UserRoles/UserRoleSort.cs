using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.UserRoles
{
    public partial class UserRoleSort : SortBase<UserRole>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? UserId { get; set; }
        public SortOperand? RoleId { get; set; }
    }
}
