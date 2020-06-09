using System.Threading.Tasks;
using Domain.Commands;
using Domain.Queries;
using EventSourcing.Sample.Tasks.Contracts.Accounts.Commands;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EventSourcing.Sample.Web.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController: Controller
    {
        private readonly ICommandBus _commandBus;
        private readonly IQueryBus _queryBus;

        public TransactionsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            _commandBus = commandBus;
            _queryBus = queryBus;
        }

        // POST api/values
        [HttpPost]
        public Task Post([FromBody]MakeTransfer command)
        {
            return _commandBus.Send(command);
        }
    }
}
