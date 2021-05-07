using System;

namespace EventSourcing.Sample.Transactions.Domain.Accounts
{
    public interface IAccountNumberGenerator
    {
        string Generate();
    }

    public class RandomAccountNumberGenerator: IAccountNumberGenerator
    {
        private Random random = new Random();

        public string Generate()
        {
            return string.Format("{0:00} {0:00000} {0:00000} {0:00000} {0:00000} {0:00000} {0:00000}",
                random.Next(99), random.Next(9999), random.Next(9999), random.Next(9999), random.Next(9999), random.Next(9999), random.Next(9999));
        }
    }
}
