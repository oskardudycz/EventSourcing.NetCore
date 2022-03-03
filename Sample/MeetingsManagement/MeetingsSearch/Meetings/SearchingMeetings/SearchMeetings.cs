using Core.ElasticSearch.Indices;
using Core.Queries;

namespace MeetingsSearch.Meetings.SearchingMeetings;

public class SearchMeetings: IQuery<IReadOnlyCollection<Meeting>>
{
    public string Filter { get; }

    public SearchMeetings(string filter)
    {
        Filter = filter;
    }
}

internal class HandleSearchMeetings: IQueryHandler<SearchMeetings, IReadOnlyCollection<Meeting>>
{
    private const int MaxItemsCount = 1000;

    private readonly Nest.IElasticClient elasticClient;

    public HandleSearchMeetings(
        Nest.IElasticClient elasticClient
    )
    {
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    public async Task<IReadOnlyCollection<Meeting>> Handle(SearchMeetings query, CancellationToken cancellationToken)
    {
        var response = await elasticClient.SearchAsync<Meeting>(
            s => s.Index(IndexNameMapper.ToIndexName<Meeting>())
                .Query(q => q.QueryString(d => d.Query(query.Filter))).Size(MaxItemsCount), cancellationToken);

        return response.Documents;
    }
}