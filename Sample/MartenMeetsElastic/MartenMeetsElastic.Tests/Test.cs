using Elastic.Clients.Elasticsearch;
using Marten;
using MartenMeetsElastic.Projections;

namespace MartenMeetsElastic.Tests;

public class I
{
    public required string Id { get; set; }
}

public class EP: ElasticsearchProjection<I>
{
    protected override Func<I, string> DocumentId => d => d.Id;
}

public class Test
{
    public Test()
    {
        var elasticsearchClient = new ElasticsearchClient();
        var documentStore = DocumentStore.For(options =>
        {
            options.Projections.Add<EP>(elasticsearchClient);
        });
    }
}
