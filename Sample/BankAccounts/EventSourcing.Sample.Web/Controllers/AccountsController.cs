using System;
using System.Threading.Tasks;
using Core.Commands;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Commands;
using EventSourcing.Sample.Transactions.Contracts.Accounts.Queries;
using EventSourcing.Sample.Transactions.Contracts.Accounts.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Web.Sample.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController: Controller
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
        public Task<AccountSummary> Get(Guid accountId)
        {
            return _queryBus.Send<GetAccount, AccountSummary>(new GetAccount(accountId));
        }

        // POST api/values
        [HttpPost]
        public Task Post([FromBody]CreateNewAccount command)
        {
            return _commandBus.Send(command);
        }
    }
}
