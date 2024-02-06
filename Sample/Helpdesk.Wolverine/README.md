[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Pragmatic Event Sourcing With Marten and Wolverine

It's the extended version of the [Helpdesk sample](../Helpdesk) adding Wolverine into the game.

It has:
- Simplest CQRS and Event Sourcing flow using Wolverine Endpoints,
- Cutting the number of layers to bare minimum,
- Using all Marten helpers like `WriteToAggregate`, `AggregateStream` to simplify the processing,
- Examples of all the typical Marten's projections,
- example of how and where to use C# Records, Nullable Reference Types, etc,
- No Aggregates! Commands are handled in the domain service as pure functions.

You can watch the original webinar on YouTube where I'm explaining the details of the implementation:

<a href="https://www.youtube.com/watch?v=jnDchr5eabI&list=PLw-VZz_H4iiqUeEBDfGNendS0B3qIk-ps&index=1" target="_blank"><img src="https://img.youtube.com/vi/jnDchr5eabI/0.jpg" alt="Pragmatic Event Sourcing with Marten" width="640" height="480" border="10" /></a>

And follow up about Wolverine:

<a href="https://www.youtube.com/watch?v=b-rxOLzevqQ&list=PLw-VZz_H4iiqUeEBDfGNendS0B3qIk-ps&index=9" target="_blank"><img src="https://img.youtube.com/vi/b-rxOLzevqQ/0.jpg" alt="Simplify your architecture with Wolverine" width="640" height="480" border="10" /></a>

or read the articles explaining this design:
- [Slim your aggregates with Event Sourcing!](https://event-driven.io/en/slim_your_entities_with_event_sourcing/?utm_source=event_sourcing_net)
- [Event-driven projections in Marten explained](https://event-driven.io/pl/projections_in_marten_explained/?utm_source=event_sourcing_net)
