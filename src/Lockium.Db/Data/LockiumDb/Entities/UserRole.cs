using Data.Repository;

namespace Lockium.Data.LockiumDb.Entities
{
    public partial class UserRole : IEntityKey<int>
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }

        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
