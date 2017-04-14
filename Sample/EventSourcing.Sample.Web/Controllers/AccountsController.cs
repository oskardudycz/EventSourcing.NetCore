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
        public Task<IEnumerable<AccountSummary>> Get([FromQuery]GetAccounts query)
        {
            return _queryBus.Send(query);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async void Post([FromBody]CreateNewAccount command)
        {
            await _commandBus.Send(command);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
