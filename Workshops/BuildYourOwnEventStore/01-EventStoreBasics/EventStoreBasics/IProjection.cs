using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStoreBasics
{
    public interface IProjection
    {
        Type[] Handles { get; }
        void Handle(object @event);
    }

    public abstract class Projection : IProjection
    {
        private readonly Dictionary<Type, Action<object>> Handlers = new Dictionary<Type, Action<object>>();

        public Type[] Handles => Handlers.Keys.ToArray();

        protected void Projects<TEvent>(Action<TEvent> action)
        {
            Handlers.Add(typeof(TEvent), @event => action((TEvent) @event));
        }

        public void Handle(object @event)
        {
            Handlers[@event.GetType()](@event);
        }
    }
}
