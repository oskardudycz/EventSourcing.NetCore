[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Build Your Own Event Store Self-Paced Kit

You can watch:

<a href="https://www.youtube.com/watch?v=gaoZdtQSOTo&list=PLw-VZz_H4iiqUeEBDfGNendS0B3qIk-ps&index=2" target="_blank"><img src="https://img.youtube.com/vi/gaoZdtQSOTo/0.jpg" alt="Let's build event store in one hour!" width="640" height="480" border="10" /></a>

and read:
- 📝 [Let's build event store in one hour!](https://event-driven.io/en/lets_build_event_store_in_one_hour/?utm_source=event_sourcing_net)

## Description

**Event Sourcing basics** - it teaches the event store basics by showing how to build your own Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projectsions, snapshots and finishes with the `Marten` basics. See more in [here](./01-EventStoreBasics/).

1. [Streams Table](./01-CreateStreamsTable)
2. [Events Table](./02-CreateEventsTable)
3. [Appending Events](./03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](./03-OptimisticConcurrency)
5. [Event Store Methods](./04-EventStoreMethods)
6. [Stream Aggregation](./05-StreamAggregation)
7. [Time Travelling](./06-TimeTraveling)
8. [Aggregate and Repositories](./07-AggregateAndRepository)
9. [Snapshots](./08-Snapshots)
10. [Projections](./09-Projections)
11. [Projections With Marten](./10-ProjectionsWithMarten)

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET 6 - https://dotnet.microsoft.com/download/dotnet/6.0.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/engine/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Go to gitter channel https://gitter.im/oskardudycz/szkola-event-sourcing.
9. Check https://github.com/StackExchange/Dapper/, https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
10. Open `BuildYourOwnEventStore.slnx` solution.
11. Docker useful commands

    - `docker compose up` - start dockers
    - `docker compose kill` - to stop running dockers.
    - `docker compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

12. For the first part of workshop please go to [./docker](./docker) and run: `docker compose up`.
13. Wait until all dockers got are downloaded and running.
14. You should automatically get:

    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
