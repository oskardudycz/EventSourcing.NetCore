using System.Threading.Tasks;
using Core.Commands;
using Core.Queries;
using EventSourcing.Sample.Transactions.Contracts.Transactions.Commands;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EventSourcing.Sample.Web.Controllers
{
    [Route("api/[controller]")]
    public class TransactionsController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;

        public TransactionsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            this.commandBus = commandBus;
            this.queryBus = queryBus;
        }

        // POST api/values
        [HttpPost]
        public Task Post([FromBody]MakeTransfer command)
        {
            return commandBus.Send(command);
        }
    }
}
