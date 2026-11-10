[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_net) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net)

# Event-Driven Architecture Workshop

Event-Driven Architecture sounds like a buzzword, but it's a practical way to build systems where modules react to what happened instead of calling each other directly. It's also easy to get wrong: lost messages, broken processes and no idea why a customer ended up in a weird state.

The workshop builds that knowledge step by step on a single domain, a hotel's guest stays and group checkouts. You start with modelling events and entities, then coordinate a business process across modules and make it survive duplicates and failures.

1. Events and entities: modelling what happened and the state that decides what can happen next.
2. Commands and events: intent versus facts.
3. Business processes: choreography, todo list, process managers and sagas, with their trade-offs.
4. Idempotency: handling the same message more than once without changing the outcome.
5. Consistency: not losing state changes or events when the database or the message bus fails, using the transactional outbox.

You can do the workshop as a self-paced kit. If you'd like to get full coverage with all nuances of the private workshop, feel free to contact me via [email](mailto:oskar.dudycz@gmail.com).

## Exercises

1. [Events definition](./01-EventsDefinition).
2. [Entities definition](./02-EntitiesDefinition).
3. [Business processes](./03-BusinessProcesses).
4. [Idempotency](./04-Idempotency).
5. [Consistency](./05-Consistency).

Each exercise has a full implementation in the [Solutions](./Solutions/) folder. Some go further than the exercise:

- [Business processes](./Solutions/03-BusinessProcesses): choreography, todo list, process managers and sagas.

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Clone this repository.
3. Install .NET 10.0 - https://dotnet.microsoft.com/download/dotnet/10.0.
4. Install Visual Studio, Rider or VSCode.
5. Open [EventDrivenArchitecture.slnx](./EventDrivenArchitecture.slnx) solution.

## Running

1. Build the solution. Everything runs in memory, so you don't need Docker.
2. Run the tests of the exercise you're working on, e.g. `dotnet test 01-EventsDefinition`.
3. To see the solved exercises, look into the [Solutions](./Solutions/) folder of the same solution.
