using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Tasks.Views.Accounts;
using Microsoft.AspNetCore.Mvc;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.DTOs;

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
        
        [HttpPost]
        public async Task Post([FromBody]CreateClient command)
        {
            await _commandBus.Send(command);
        }
        
        [HttpPut("{id}")]
        public async Task Put(Guid id, [FromBody]ClientInfo clientInfo)
        {
            await _commandBus.Send(new UpdateClient(id, clientInfo));
        }

        // POST api/values

        [HttpDelete("{id}")]
        public async Task Post(Guid id)
        {
            await _commandBus.Send(new DeleteClient(id));
        }
    }
}
