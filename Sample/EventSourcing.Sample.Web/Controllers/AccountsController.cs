﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Domain.Commands;
using Domain.Queries;
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
        public Task<AccountSummary> Get(Guid accountId)
        {
            return _queryBus.Send<GetAccount, AccountSummary>(new GetAccount(accountId));
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]CreateNewAccount command)
        {
            await _commandBus.Send(command);
        }
    }
}

