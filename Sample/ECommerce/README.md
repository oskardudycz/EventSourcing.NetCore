[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Practical Event Sourcing Workshop

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 6.0 - https://dotnet.microsoft.com/en-us/download/dotnet/6.0.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Go to gitter channel (ENG) https://gitter.im/EventSourcing-NetCore/community (POL) https://gitter.im/oskardudycz/szkola-event-sourcing.
9. Check https://github.com/StackExchange/Dapper/, https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
10. Open `PracticalEventSourcing.sln` solution.
11. Docker useful commands

    - `docker compose up` - start dockers
    - `docker compose kill` - to stop running dockers.
    - `docker compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

12. Go to [docker](./docker) and run: `docker compose up`.
13. Wait until all dockers got are downloaded and running.
14. You should automatically get:
    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `pgadmin4@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
    - Kafka
    - Kafka ide for browsing topics. Available at: http://localhost:8080/ui/clusters/local/topics
