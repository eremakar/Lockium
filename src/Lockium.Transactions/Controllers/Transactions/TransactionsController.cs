using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Lockium.Data.LockiumDb.Entities;
using Lockium.Data.LockiumDb.Entities.Transactions;
using Lockium.Models.Dtos.Transactions;
using Lockium.Models.Queries.Transactions.Transactions;
using Lockium.Mappings.Transactions;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Transactions.Controllers.Transactions
{
    /// <summary>
    /// Транзакция, создаётся при брони или заказе
    /// </summary>
    [Route("/api/v1/transactions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class TransactionsController : RestControllerBase2<Transaction, long, TransactionDto, TransactionQuery, TransactionMap>
    {
        public TransactionsController(ILogger<RestServiceBase<Transaction, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            TransactionMap transactionMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Transactions",
                transactionMap)
        {
        }

        private static IQueryable<Transaction> TransactionQuery(IQueryable<Transaction> query) =>
            query.Select(t => new Transaction
            {
                Id = t.Id,
                State = t.State,
                SourceType = t.SourceType,
                CreatedTime = t.CreatedTime,
                ClientId = t.ClientId,
                ReservationId = t.ReservationId,
                OrderId = t.OrderId,
                Client = t.Client == null
                    ? null
                    : new User
                    {
                        UserName = t.Client.UserName,
                    },
                Reservation = t.Reservation,
                Order = t.Order,
            });

        /// <summary>
        /// Search of Transaction using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of transactions</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/transactions/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<TransactionDto>> SearchAsync([FromBody] TransactionQuery query)
        {
            return await SearchUsingEfAsync(query, TransactionQuery);
        }

        /// <summary>
        /// Get the transaction by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Transaction data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/transactions/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<TransactionDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, TransactionQuery);
        }

    }
}
