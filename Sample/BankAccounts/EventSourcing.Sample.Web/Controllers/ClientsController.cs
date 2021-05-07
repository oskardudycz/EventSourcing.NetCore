using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Commands;
using Core.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Transactions.Views.Clients;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Sample.Web.Controllers
{
    [Route("api/[controller]")]
    public class ClientsController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;

        public ClientsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            this.commandBus = commandBus;
            this.queryBus = queryBus;
        }

        [HttpGet]
        public Task<List<ClientListItem>> Get()
        {
            return queryBus.Send<GetClients, List<ClientListItem>>(new GetClients());
        }

        [HttpGet("{id}")]
        public Task<ClientItem> Get(Guid id)
        {
            return queryBus.Send<GetClient, ClientItem>(new GetClient(id));
        }

        [HttpGet]
        [Route("{id}/accounts")]
        public Task<IEnumerable<AccountSummary>> GetAccounts(Guid id)
        {
            return queryBus.Send<GetAccounts, IEnumerable<AccountSummary>>(new GetAccounts(id));
        }

        [HttpGet]
        [Route("{id}/view")]
        public Task<ClientView> GetClientView(Guid id)
        {
            return queryBus.Send<GetClientView, ClientView>(new GetClientView(id));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateClient command)
        {
            await commandBus.Send(command);

            return Created("api/Clients", command.Id);
        }

        [HttpPut("{id}")]
        public Task Put(Guid id, [FromBody]ClientInfo clientInfo)
        {
            return commandBus.Send(new UpdateClient(id, clientInfo));
        }

        // POST api/values

        [HttpDelete("{id}")]
        public Task Post(Guid id)
        {
            return commandBus.Send(new DeleteClient(id));
        }
    }
}
