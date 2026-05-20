using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Data.Repository.Helpers
{
    public static class TransactionHelper
    {
        public static TransactionScope CreateScope(TimeSpan? timeout = null)
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = timeout ?? new TimeSpan(0, 0, 0, 30)
            }, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static async Task ExecuteTransaction<TDbContext>(this TDbContext db, Func<Task> transactionOperation)
            where TDbContext : DbContext
        {
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                await transactionOperation();

                await transaction.CommitAsync();
            });

        }
    }
}
