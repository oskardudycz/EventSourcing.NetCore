using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Views.Accounts;
using EventSourcing.Sample.Tasks.Contracts.Accounts;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Tasks.Views.Account;

namespace EventSourcing.Web.Sample.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly ICommandBus _commandBus;
        private readonly IQueryBus _queryBus;

        public AccountsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            _commandBus = commandBus;
            _queryBus = queryBus;
        }

        // GET api/values
        [HttpGet]
        [Route("clientid={client_id}")]
        public Task<IEnumerable<AccountSummary>> GetClientAccounts(Guid client_id)
        {
            return _queryBus.Send(new GetAccounts(client_id));
        }

        // GET api/values
        [HttpGet]
        [Route("accountid={account_id}")]
        public Task<AccountSummary> Get(Guid account_id)
        {
            return _queryBus.Send(new GetAccount(account_id));
        }

        // POST api/values
        [HttpPost]
        public async void Post([FromBody]CreateNewAccount command)
        {
            await _commandBus.Send(command);
        }
    }
}

