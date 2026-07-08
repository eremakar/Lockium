using Lockium.Models.Dtos.Orders;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Lockium.Client.Api.Controllers.Orders
{
    public partial class OrdersController
    {
        [NonAction]
        public override Task<object> AddAsync(OrderDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> UpdateAsync(OrderDto request) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<object> RemoveAsync(long key) =>
            throw new NotSupportedException();

        [NonAction]
        public override Task<IActionResult> PatchAsync(long id, JsonPatchDocument<OrderDto> patch) =>
            throw new NotSupportedException();
    }
}
