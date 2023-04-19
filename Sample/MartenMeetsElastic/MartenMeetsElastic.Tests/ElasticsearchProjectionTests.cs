using Core.ElasticSearch.Indices;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FluentAssertions;
using JasperFx.Core;
using Marten;
using Marten.Events;
using Marten.Events.Daemon;
using MartenMeetsElastic.Projections;
using Xunit;

namespace MartenMeetsElastic.Tests;

public record OrderInitiated(
    string OrderId,
    string OrderNumber,
    UserInfo User
);

public record OrderShipmentAddressAssigned(
    string OrderId,
    string ShipmentAddress
);

public record OrderCompleted(
    string OrderId,
    string OrderNumber,
    string UserName
);

public record UserInfo(
    string Id,
    string UserName
);

public record Order(
    string Id,
    string OrderNumber,
    UserInfo User,
    string Status,
    string? ShipmentAddress
);

public class OrderProjectionRaw: ElasticsearchProjection
{
    protected override string IndexName => "Document";

    public OrderProjectionRaw()
    {
        Projects<OrderInitiated>();
        Projects<OrderShipmentAddressAssigned>();
        Projects<OrderCompleted>();
    }


    protected override Task ApplyAsync(ElasticsearchClient client, object[] events)
    {
        // (...) TODO
    }
}

public class OrderProjection: ElasticsearchProjection<Order>
{
    public OrderProjection()
    {
        DocumentId(o => o.Id);

        Projects<OrderInitiated>(e => e.OrderId, Apply);
        Projects<OrderShipmentAddressAssigned>(e => e.OrderId, Apply);
        Projects<OrderCompleted>(e => e.OrderId, Apply);
    }

    private Order Apply(Order order, OrderInitiated @event) =>
        order with
        {
            Id = @event.OrderId,
            OrderNumber = @event.OrderNumber,
            User = @event.User
        };

    private Order Apply(Order order, OrderShipmentAddressAssigned @event) =>
        order with
        {
            ShipmentAddress = @event.ShipmentAddress
        };

    private Order Apply(Order order, OrderCompleted @event) =>
        order with
        {
            Status = "Completed"
        };
}

public class MeetsElasticTest: MartenMeetsElasticTest
{
    protected override void Options(StoreOptions options)
    {
        options.Projections.Add<OrderProjection>(elasticClient);
    }

    [Fact]
    public async Task ShouldProjectEvents_ToElasticsearch()
    {
        await AppendEvents("order1", new OrderInitiated("order1", "ORD/123", new UserInfo("user1", "user1")));

        await StartDaemon();

        await daemon.Tracker.WaitForShardState(new ShardState("MartenMeetsElastic.Tests.OrderProjection:All", 1), 15.Seconds());

        var searchResponse = await elasticClient.SearchAsync<Order>(s => s
            .Index(IndexNameMapper.ToIndexName<Order>())
            .Query(q => q.Ids(new IdsQuery { Values = new Ids("order1") }))
        );

        searchResponse.IsValidResponse.Should().BeTrue();
        searchResponse.Documents.Should().HaveCount(1);
    }
}
