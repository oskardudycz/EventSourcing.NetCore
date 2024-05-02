using Core.ElasticSearch.Indices;
using Core.Queries;
using Elastic.Clients.Elasticsearch;

namespace MeetingsSearch.Meetings.SearchingMeetings;

public class SearchMeetings(string filter)
{
    public string Filter { get; } = filter;
}

internal class HandleSearchMeetings(ElasticsearchClient elasticClient)
    : IQueryHandler<SearchMeetings, IReadOnlyCollection<Meeting>>
{
    private const int MaxItemsCount = 1000;

    private readonly ElasticsearchClient elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

    public async Task<IReadOnlyCollection<Meeting>> Handle(SearchMeetings query, CancellationToken cancellationToken)
    {
        var response = await elasticClient.SearchAsync<Meeting>(
            s => s.Index(IndexNameMapper.ToIndexName<Meeting>())
                .Query(q => q.QueryString(d => d.Query(query.Filter))).Size(MaxItemsCount), cancellationToken);

        return response.Documents;
    }
}
