using System;
using System.Collections.Generic;

namespace EventSourcing.Sample.Transactions.Views.Clients
{
    public class ClientView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public List<string> AccountsNumbers { get; set; } = default!;
    }
}
