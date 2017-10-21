using Domain.Commands;

namespace EventSourcing.Sample.Clients.Contracts.Users.Commands
{
    public class CreateUser : ICommand
    {
        public string Name { get; }
        public string Email { get; }

        public CreateUser(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
