using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.Roles
{
    public partial class RoleFilter : FilterBase<Role>
    {
        public FilterOperand<int>? Id { get; set; }
        public FilterOperand<string>? Name { get; set; }
        public FilterOperand<string>? Code { get; set; }
    }
}
