﻿using System;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.ValueObjects;
using System.Collections.Generic;

namespace EventSourcing.Sample.Tasks.Views.Account
{
    public class GetAccount : IQuery<AccountSummary>
    {
        public Guid AccountId { get; private set; }
        public GetAccount(Guid accountId)
        {
            AccountId = accountId;
        }

    }
}
