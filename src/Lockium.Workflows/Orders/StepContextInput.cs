using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Orders;

namespace Lockium.Workflows.Orders
{
    public class StepContextInput
    {
        public long Id { get; set; }
        public int PreviousState { get; set; }
        public int NextState { get; set; }
        public LockiumDbContext Db { get; set; } = null!;
        public OrderDto? Request { get; set; }
    }
}
