using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Tasks.Views.Accounts;
using EventSourcing.Sample.Transactions.Views.Clients;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Sample.Web.Controllers
{
    [Route("api/[controller]")]
    public class ClientsController : Controller
    {
        private readonly ICommandBus _commandBus;
        private readonly IQueryBus _queryBus;

        public ClientsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            _commandBus = commandBus;
            _queryBus = queryBus;
        }

        [HttpGet]
        public Task<List<ClientListItem>> Get()
        {
            return _queryBus.Send<GetClients, List<ClientListItem>>(new GetClients());
        }

        [HttpGet("{id}")]
        public Task<ClientItem> Get(Guid id)
        {
            return _queryBus.Send<GetClient, ClientItem>(new GetClient(id));
        }

        [HttpGet]
        [Route("{id}/accounts")]
        public Task<IEnumerable<AccountSummary>> GetAccounts(Guid id)
        {
            return _queryBus.Send<GetAccounts, IEnumerable<AccountSummary>>(new GetAccounts(id));
        }

        [HttpGet]
        [Route("{id}/view")]
        public Task<ClientView> GetClientView(Guid id)
        {
            return _queryBus.Send<GetClientView, ClientView>(new GetClientView(id));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateClient command)
        {
            command.Id = command.Id ?? Guid.NewGuid();

            await _commandBus.Send(command);

            return Created("api/Clients", command.Id);
        }

        [HttpPut("{id}")]
        public Task Put(Guid id, [FromBody]ClientInfo clientInfo)
        {
            return _commandBus.Send(new UpdateClient(id, clientInfo));
        }

        // POST api/values

        [HttpDelete("{id}")]
        public Task Post(Guid id)
        {
            return _commandBus.Send(new DeleteClient(id));
        }
    }
}