using Data.Repository;
using Lockium.Data.LockiumDb.Entities.Transactions;

namespace Lockium.Models.Queries.Transactions.Transactions
{
    public partial class TransactionQuery : QueryBase<Transaction, TransactionFilter, TransactionSort>
    {
    }
}
