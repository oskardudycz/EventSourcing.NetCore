[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Practical Event Sourcing Workshop

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET 10 - https://dotnet.microsoft.com/en-us/download/dotnet/10.0.
3. Install Visual Studio 2022, Rider or VSCode.
4. Install docker - https://docs.docker.com/engine/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Check [Marten documentation](https://martendb.io/).
9. Open `PracticalEventSourcing.slnx` solution.
10. Docker useful commands

    - `docker compose --profile all up` - start dockers
    - `docker compose --profile all kill` - to stop running dockers.
    - `docker compose --profile all down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

11. Go to [docker](./docker) and run: `docker compose --profile all up`.
12. Wait until all containers are downloaded and running.
13. You should automatically get:
    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `pgadmin4@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `postgres`
    - Kafka
    - Kafka ide for browsing topics. Available at: http://localhost:8080/ui/clusters/local/topics

Alternatively, run the [ECommerce.AppHost](./ECommerce.AppHost/) project. It uses [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) to start PostgreSQL with PgAdmin, Kafka, and the Carts, Payments and Orders APIs.
