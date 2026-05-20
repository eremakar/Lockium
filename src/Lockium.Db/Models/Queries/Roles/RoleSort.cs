using Data.Repository;
using Lockium.Data.LockiumDb.Entities;

namespace Lockium.Models.Queries.Roles
{
    public partial class RoleSort : SortBase<Role>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Name { get; set; }
        public SortOperand? Code { get; set; }
    }
}
