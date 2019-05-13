using System;

namespace EventSourcing.Sample.Tasks.Contracts.Transactions
{
    public class Inflow: ITransaction
    {
        public decimal Ammount { get; }

        public DateTime Timestamp { get; }

        public Inflow(decimal ammount, DateTime timestamp)
        {
            Ammount = ammount;
            Timestamp = timestamp;
        }
    }
}
