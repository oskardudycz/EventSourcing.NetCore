using System;

namespace EventSourcing.Sample.Transactions.Contracts.Transactions
{
    public class Outflow: ITransaction
    {
        public decimal Amount { get; }

        public DateTime Timestamp { get; }

        public Outflow(decimal amount, DateTime timestamp)
        {
            Amount = amount;
            Timestamp = timestamp;
        }
    }
}
