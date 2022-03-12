# Build Your Own Event Store Self-Paced Kit

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET 5 - https://dotnet.microsoft.com/download/dotnet/5.0.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/engine/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Check https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
9. Open `MeetingsManagement.sln` solution.
10. Docker useful commands

    - `docker-compose up` - start dockers
    - `docker-compose kill` - to stop running dockers.
    - `docker-compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

11. Wait until all dockers got are downloaded and running.
12. You should automatically get:

    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
    - Kafka
    - Kafka ide for browsing topics. Available at: http://localhost:8080/ui/clusters/local/topics
    - ElasticSearch
    - Kibana for browsing ElasticSearch - http://localhost:5601

## Description

It's a real world sample of the microservices written in Event-Driven design. It explains the topics of modularity, eventual consistency. Shows practical usage of WebApi, Marten as Event Store, Kafka as Event bus and ElasticSearch as one of the read stores. See more in [here](./02-EventSourcingAdvanced/).

1. [Meetings Management Module](./MeetingsManagement) - module responsible for creating, updating meetings details. Written in `Marten` in **Event Sourcing** pattern. Provides both write model (with Event Sourced aggregates) and read model with projections.
2. [Meetings Search Module](./MeetingsSearch) - responsible for searching and advanced filtering. Uses `ElasticSearch` as a storage (because of it's advanced searching capabilities). It's a read module that's listening for the events published by Meetings Management Module.
