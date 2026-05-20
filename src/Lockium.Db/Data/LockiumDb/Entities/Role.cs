using Data.Repository;

namespace Lockium.Data.LockiumDb.Entities
{
    public partial class Role : IEntityKey<int>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }
}
