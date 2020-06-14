using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using MeetingsSearch.Meetings.Queries;

namespace MeetingsSearch.Meetings
{
    internal class MeetingQueryHandler: IQueryHandler<SearchMeetings, IReadOnlyCollection<Meeting>>
    {
        private const int MaxItemsCount = 1000;

        private readonly Nest.IElasticClient elasticClient;

        public MeetingQueryHandler(
            Nest.IElasticClient elasticClient
        )
        {
            this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        public async Task<IReadOnlyCollection<Meeting>> Handle(SearchMeetings query, CancellationToken cancellationToken)
        {
            var response = await elasticClient.SearchAsync<Meeting>(
                s => s.Query(q => q.QueryString(d => d.Query(query.Filter))).Size(MaxItemsCount), cancellationToken);

            return response.Documents;
        }
    }
}
