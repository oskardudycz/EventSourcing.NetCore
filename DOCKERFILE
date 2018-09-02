# Build image
FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /app

# Copy files
COPY . ./

RUN dotnet restore
RUN dotnet build

WORKDIR /app/Sample/EventSourcing.Sample.Web
RUN dotnet publish -c Debug -o out

# Build runtime image
FROM microsoft/dotnet:2.1-sdk
WORKDIR /app
COPY --from=builder /app/Sample/EventSourcing.Sample.Web/out .
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet", "EventSourcing.Sample.Web.dll"]