[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Projecting Marten events to Elasticsearch

Sample for the article [Projecting Marten events to Elasticsearch](https://event-driven.io/en/projecting_from_marten_to_elasticsearch/?utm_source=event_sourcing_net).

Projecting straight into Postgres gives you enough power for most common cases. Yet, sometimes you need to do more. This sample shows how to build an external projection in [Marten](https://martendb.io/), using Elasticsearch as an example.

Assumptions:

- even though [Postgres has full-text search capabilities](https://martendb.io/documents/full-text.html), we'd like to use Elasticsearch, as it's a database natively built for such needs,
- as Elasticsearch has eventual consistency, and we don't want to fall into the [Two Phase Commit](https://martinfowler.com/articles/patterns-of-distributed-systems/two-phase-commit.html) issue, we use asynchronous processing,
- we don't introduce additional messaging tooling to apply events stored in Marten,
- [Marten's async daemon](https://martendb.io/events/projections/async-daemon.html) guarantees the sequential order of event processing and that all events will be delivered.

The sample has:

- [ElasticsearchProjection](./MartenMeetsElastic/Projections/ElasticsearchProjection.cs): base class built on Marten subscriptions (`SubscriptionBase`). The article implemented `IProjection` instead. It makes sure that the index exists with explicit mappings, selects only the event types the projection handles, processes them in batches, and reports failures to the subscription controller,
- [ElasticsearchProjectionConfig](./MartenMeetsElastic/Projections/ElasticsearchProjectionConfig.cs): registration helper that subscribes the projection to Marten events and injects the `ElasticsearchClient`,
- [ElasticsearchProjectionTests](./MartenMeetsElastic.Tests/ElasticsearchProjectionTests.cs): projection of the order flow events (`OrderInitiated`, `OrderShipmentAddressAssigned`, `OrderCompleted`) into the `Order` document.

## Running

1. Start Postgres and Elasticsearch from the repository root: `docker compose --profile ci up`.
2. Run the tests: `dotnet test Sample/MartenMeetsElastic/MartenMeetsElastic.Tests`.
