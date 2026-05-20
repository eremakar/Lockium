using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Orders;

namespace Lockium.Models.Queries.Orders.Orders
{
    public partial class OrderQuery : QueryBase<Order, OrderFilter, OrderSort>
    {
    }
}
