using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repository.Workflow
{
    public abstract class TransactionStepBase<TStepContext> : StepBase<TStepContext>
        where TStepContext : TransactionStepContextBase<TStepContext>, new()
    {
        public TransactionStepBase() : base()
        {
        }

        public TransactionStepBase(TStepContext stepContext) : 
            base(stepContext)
        {
        }

        protected abstract Task RunTransactionAsync(DbContext db);

        protected abstract DbContext GetDb();

        public override async Task Run()
        {
            var db = GetDb();
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                await RunTransactionAsync(db);

                await transaction.CommitAsync();
            });
        }

        /// <summary>
        /// Runs the step as part of a chain. By default, calls RunTransactionAsync.
        /// Derived classes can override this method to customize behavior for chained execution.
        /// </summary>
        public virtual async Task RunChained()
        {
            var db = GetDb();
            var strategy = db.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                await RunTransactionAsync(db);

                await transaction.CommitAsync();
            });
        }
    }
}
