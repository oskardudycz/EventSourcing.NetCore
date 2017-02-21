using System;

namespace EventSourcing.Sample.Tasks.Contracts.Transactions
{
    public class Outflow : ITransaction
    {
        public decimal Ammount { get; }

        public DateTime Timestamp { get; }

        public Outflow(decimal ammount, DateTime timestamp)
        {
            Ammount = ammount;
            Timestamp = timestamp;
        }
    }
}
