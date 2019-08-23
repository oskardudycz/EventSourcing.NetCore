using System;
using MeetingsSearch.Meetings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace MeetingsSearch
{
    public static class ElasticSearchConfig
    {
        public static void AddElasticsearch(
            this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["elasticsearch:url"];
            var defaultIndex = configuration["elasticsearch:index"];

            var settings = new ConnectionSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .DefaultMappingFor<Meeting>(m => m
                    //.Ignore(p => p.IsPublished)
                    .PropertyName(p => p.Id, "id")
                    .PropertyName(p => p.Name, "name")
                );
            //                .DefaultMappingFor<Comment>(m => m
            //                    .Ignore(c => c.Email)
            //                    .Ignore(c => c.IsAdmin)
            //                    .PropertyName(c => c.ID, "id")
            //                );

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }
    }
}
