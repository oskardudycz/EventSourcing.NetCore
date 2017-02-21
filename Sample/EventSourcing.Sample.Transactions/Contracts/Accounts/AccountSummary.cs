using System;
namespace EventSourcing.Sample.Tasks.Contracts.Accounts
{
    public class AccountSummary
    {
        public Guid AccountId { get; set; }
        public decimal Balance { get; internal set; }
        public Guid ClientId { get; set; }

        public int TransactionsCount { get; set; }
    }
}
