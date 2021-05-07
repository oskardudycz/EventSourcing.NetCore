using System;
using Core.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public record CreateClient(
        Guid Id,
        ClientInfo Data
    ): ICommand;
}
