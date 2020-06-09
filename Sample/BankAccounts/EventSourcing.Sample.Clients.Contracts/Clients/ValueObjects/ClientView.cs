using System;
using System.Collections.Generic;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> AccountsNumbers { get; set; }
    }
}