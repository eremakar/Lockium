using Lockium.Data.LockiumDb.DatabaseContext;
using Lockium.Models.Dtos.Reservations;

namespace Lockium.Workflows.Reservations
{
    public class StepContextInput
    {
        public long Id { get; set; }
        public int PreviousState { get; set; }
        public int NextState { get; set; }
        public LockiumDbContext Db { get; set; } = null!;
        public ReservationDto? Request { get; set; }
    }
}
