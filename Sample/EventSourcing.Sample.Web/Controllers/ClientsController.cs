using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using EventSourcing.Sample.Tasks.Views.Accounts;
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

        // GET api/values
        [HttpGet]
        [Route("{clientId}/accounts")]
        public Task<IEnumerable<AccountSummary>> Get(Guid clientId)
        {
            return _queryBus.Send(new GetAccounts(clientId));
        }
    }
}
