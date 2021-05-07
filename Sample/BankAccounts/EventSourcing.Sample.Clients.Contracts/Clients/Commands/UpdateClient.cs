using System;
using Core.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;

namespace EventSourcing.Sample.Clients.Contracts.Clients.Commands
{
    public record UpdateClient(
        Guid Id,
        ClientInfo Data
    ): ICommand;
}
