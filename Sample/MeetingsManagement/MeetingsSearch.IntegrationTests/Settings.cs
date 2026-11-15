using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(MeetingsSearch.IntegrationTests.TestElasticsearch))]

namespace MeetingsSearch.IntegrationTests;

public class TestElasticsearch(): ElasticsearchContainerFixture("Elasticsearch__Url");
