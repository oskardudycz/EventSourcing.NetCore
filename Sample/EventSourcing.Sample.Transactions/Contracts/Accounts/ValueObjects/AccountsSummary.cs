namespace EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects
{
    public class AccountsSummary
    {
        public int TotalCount { get; set; }

        public decimal TotalBalance { get; set; }

        public int TotalTransactionsCount { get; set; }
    }
}
