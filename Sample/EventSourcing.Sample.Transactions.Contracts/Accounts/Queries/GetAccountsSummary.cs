using Domain.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.Queries
{
    public class GetAccountsSummary: IQuery<AllAccountsSummary>
    {
    }
}
