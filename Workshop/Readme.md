# Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 2.2 - https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.108-windows-x64-installer.
3. Install Visual Studio (suggested 2017) or Rider.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Go to gitter channel https://gitter.im/oskardudycz/szkola-event-sourcing.
9. Check https://github.com/StackExchange/Dapper/, https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
10. Go to [docker folder](../docker/) open CMD and run `docker-compose up`. Other useful commands are:
* `docker-compose kill` to stop running dockers. 
* `docker-compose down -v` to clean stopped dockers. 
* `docker ps` - for showing running dockers
* `docker ps -a` - to show all dockers (also stopped)
11. Wait until all dockers got are downloaded and running.
12. You should automatically get:
* Postgres DB running
* PG Admin - IDE for postgres. Available at: http://localhost:5050.
  * Login: `pgadmin4@pgadmin.org`, Password: `admin`
  * To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
* Kafka
* Kafka ide for browsing topics. Available at: http://localhost:8000
* ElasticSearch
* Kibana for browsing ElasticSearch - http://localhost:5601
