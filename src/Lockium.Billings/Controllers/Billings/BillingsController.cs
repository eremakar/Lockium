using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Billings;
using Lockium.Models.Dtos.Billings;
using Lockium.Models.Queries.Billings.Billings;
using Lockium.Mappings.Billings;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Billings.Controllers.Billings
{
    /// <summary>
    /// Данные по времени брони и заказа
    /// </summary>
    [Route("/api/v1/billings")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class BillingsController : RestControllerBase2<Billing, long, BillingDto, BillingQuery, BillingMap>
    {
        public BillingsController(ILogger<RestServiceBase<Billing, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            BillingMap billingMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Billings",
                billingMap)
        {
        }

        /// <summary>
        /// Search of Billing using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of billings</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/billings/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<BillingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<BillingDto>> SearchAsync([FromBody] BillingQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Transaction).
                Include(_ => _.Reservation).
                Include(_ => _.Order));
        }

        /// <summary>
        /// Get the billing by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Billing data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/billings/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(BillingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<BillingDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Transaction).
                Include(_ => _.Reservation).
                Include(_ => _.Order));
        }

    }
}
