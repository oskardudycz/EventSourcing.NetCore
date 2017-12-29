using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace CQRS.Tests.TestsInfrasructure
{
    internal class ServiceLocator
    {
        private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

        public void Register(Type type, params object[] implementations)
            => Services.Add(type, implementations.ToList());

        public List<object> Get(Type type)
        {
            return Services[type];
        }

        public void RegisterCommandHandler<TCommand, TCommandHandler>(TCommandHandler commandHandler)
            where TCommand : IRequest
            where TCommandHandler : IRequestHandler<TCommand>
        {
            Register(typeof(IRequestHandler<TCommand>), commandHandler, commandHandler);
            //Registration needed internally by MediatR
            Register(typeof(IPipelineBehavior<TCommand, Unit>), new object[] { });
        }

        public void RegisterQueryHandler<TQuery, TResponse>(IRequestHandler<TQuery, TResponse> queryHandler)
            where TQuery : IRequest<TResponse>
        {
            Register(typeof(IRequestHandler<TQuery, TResponse>), queryHandler);
            //Registration needed internally by MediatR
            Register(typeof(IPipelineBehavior<TQuery, TResponse>), new object[] { });
        }

        public IMediator GetMediator()
        {
            return new Mediator(
                    type => Get(type).FirstOrDefault(),
                    type => Get(type));
        }
    }
}