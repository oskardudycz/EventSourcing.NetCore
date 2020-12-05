using System;

namespace EventSourcing.Sample.Transactions.Contracts.Transactions
{
    public class Inflow: ITransaction
    {
        public decimal Amount { get; }

        public DateTime Timestamp { get; }

        public Inflow(decimal amount, DateTime timestamp)
        {
            Amount = amount;
            Timestamp = timestamp;
        }
    }
}
