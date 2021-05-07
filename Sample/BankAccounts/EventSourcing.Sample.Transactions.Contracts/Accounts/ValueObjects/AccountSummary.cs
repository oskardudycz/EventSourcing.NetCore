using System;

namespace EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects
{
    public class AccountSummary
    {
        public Guid AccountId { get; set; }
        public Guid ClientId { get; set; }
        public string Number { get; set; } = default!;
        public decimal Balance { get; set; }
        public int TransactionsCount { get; set; }
    }
}
