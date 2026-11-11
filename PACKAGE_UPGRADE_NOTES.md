Package upgrades rechecked against NuGet on 2026-11-05: all 187 projects (167 in the main solution and 20 outside it), shared MSBuild files, and 95 declared package and package SDK versions were audited. All 1,119 explicit PackageReference declarations used the central versions, with no local version overrides or missing central entries. The subsequent compatibility fixes add a Debug-only reference to WolverineFx.RuntimeCompilation 6.61.0 and Microsoft.Extensions.Logging 10.0.14 for MediatR registration, bringing the declarations to 97 package/SDK versions and 1,121 explicit PackageReference entries across projects and shared build files. Both added versions were verified against NuGet. Unquote is the only version below the latest stable release, due to the dependency constraint described below. The recheck found Quartz 4.7.2; the central version was updated from 4.7.1.

Validation on 2026-11-06: the full main CI test command passes with 744 passed, zero failures, and 1 skipped. All 153 tests in the three affected suites also pass after the approved nine assertion corrections. The business implementations are unchanged. The separate Marten Testcontainers CI suite previously passed with 22 passed and 4 skipped; both Helpdesk Docker builds and the Wolverine static production HTTP smoke test also passed. Workshop test variants use explicit, predictable schemas documented in both exercise READMEs, and retain their events for inspection. The Elasticsearch client/server mismatch is corrected and its previously hanging test passes.

Ogooreck was subsequently upgraded to 0.10.0 on 2026-11-04 after its new release was published.

75 existing central package versions and the Aspire AppHost SDK were upgraded. Remaining direct dependencies use their latest stable versions except the constrained Unquote dependency described below. Obsolete packages were removed, and Entity Framework Core Relational was explicitly aligned with the other EF Core packages.

Notable versions:

- Marten / Marten.AspNetCore 9.57.0; Wolverine 6.61.0.
- AutoMapper 16.2.0; MediatR 14.2.0; FluentAssertions 8.11.0.
- Aspire packages and AppHost SDK 13.6.4.
- Microsoft ASP.NET Core / EF Core packages 10.0.14; Npgsql 10.0.3.
- Elasticsearch client 9.5.3; Quartz 4.7.2; OpenTelemetry 1.20.1.
- Ogooreck 0.10.0, retaining FluentAssertions 8.11.0.

Mutable workshop carts use a single public `Apply(ShoppingCartEvent)` dispatcher and retain their private event-specific handlers. Mixed carts retain their public union dispatcher and private handlers. Immutable carts initialize their complete state in the `ShoppingCartOpened` branch, retaining private parameterless constructors. Discovery of existing public document handlers replaces removed `ProjectEvent` forwarding registrations, as requested. See the [Marten migration guide](https://martendb.io/migration-guide).

The eight mutable/mixed carts retain public parameterless constructors, approved on 2026-11-05, so Marten runs the collection and pending-event queue initializers. The four generic workshop bases use the approved `Aggregate<TEvent> : Aggregate<TEvent, Guid>` layout, with identity and implementation in `Aggregate<TEvent, TId>`. Its public abstract `Apply(TEvent)` requires each concrete mutable cart to provide a public override that Marten can discover. The generic identity avoids Marten 9.57's source generation failure for open generic bases with a concrete Guid identity. The typed pending-event queue is retained. Nested workshop test models use internal visibility, and `OrderDetails.Events` remains getter-only with System.Text.Json's `JsonObjectCreationHandling.Populate`.

All direct Newtonsoft package references and application usages were removed. Serialization uses System.Text.Json, with explicit constructor/property annotations and snapshot round-trip regression tests. Ogooreck 0.10.0 also removes its Newtonsoft dependency. Some upstream packages still depend on Newtonsoft, including Equinox.CosmosStore, Aspire, and EF Core design tooling.

Marten-only CLI code generation and obsolete static-loading settings were removed as required by the Marten 9 migration guide. Wolverine retains build-time code generation. Its runtime compiler dependency is Debug-only; Release publishes exclude it, and Production uses static loading with generated-type validation. Debug integration tests use Development; Release tests select Production through per-project run settings and a local test database connection. CI generates Wolverine code before restoring/building Release. Both Docker workflows also run when central package versions change.

Elasticsearch and Kibana images are aligned to 9.5.6, approved on 2026-11-05, because the Elasticsearch 9 client is incompatible with the former 8.13.2 server. Fresh CI stores work with these images. Existing Elasticsearch 8 data needs the vendor's staged upgrade process; the image change does not migrate or delete existing volumes.

Quartz's hosting, dependency injection, and System.Text.Json support now come from its main package. Jaeger export uses OTLP, with the local Jaeger collector ports enabled in docker-compose.yml. See the [Quartz migration guide](https://www.quartz-scheduler.net/documentation/quartz-4.x/migration-guide.html) and [OpenTelemetry exporter migration](https://opentelemetry.io/blog/2023/jaeger-exporter-collector-migration/).

Known dependency constraints:

- Unquote remains 6.1.0: Equinox.DynamoStore 4.1.4 depends on FSharp.AWS.DynamoDB 0.13.2-beta, which requires Unquote `[6.1.0, 6.2.0)`. The latest Unquote 7.0.1 does not satisfy that constraint.

Validation:

- Release build of EventSourcing.NetCore.slnx (167 projects), plus builds of all 20 projects outside that solution.
- Earlier selected validation recorded 64 passing tests across Marten integration tests, custom event-store workshop tests, Core, Tickets, Carts, F# property tests, solved state reconstruction exercises, trace metadata tests, and the new workshop aggregation regression. Five existing tests remained skipped.
- Direct PostgreSQL checks of the actual Carts and Tickets inline projections, snapshot reloads, and workshop live aggregation, independent of Ogooreck.
- After upgrading Ogooreck and correcting the immutable initialization and typed dispatchers, both solved Marten workshop suites and the Tickets/Carts API suites pass: 79 + 84 + 1 + 5 = 169 tests. The workshop aggregation regression is included in the 79, so this count overlaps the earlier selected validation.
- After reducing nested test-model visibility to internal, the custom event-store suite passes 21 tests with one existing skip. The actual repository `OrderDetails` getter-only event collection also round-trips through Marten's System.Text.Json serializer.
- After applying the approved generic base layout, the full Release solution build passes with zero errors and the existing ASPIRE010 warning. Both solved Marten workshop suites pass again (79 + 84), including the generic-base replay and pending-event regression.
- Before the assertion corrections, the full main CI command reported 735 passed, 9 failed, and 1 skipped. After the user approved the nine test-only corrections, the full command passed on 2026-11-06 with 744 passed, zero failures, and 1 skipped. The separate Marten Testcontainers CI suite passed with 22 passed and 4 skipped.
- MediatR 14 uses its documented AddMediatR registration with AddLogging, replacing direct Mediator construction; all six EventPipelines tests pass. The Shipments test now sends its request and observes events through the same injected factory; its acceptance test passes without changing the assertion.
- PostgreSQL container reuse is retained. Fixture teardown disposes its data source without stopping a container other fixtures/processes may still use. A new regression verifies that another fixture sharing the container remains usable after teardown. All four EF tests and the new regression pass in the full run.
- PointOfSales explicitly retains Rich append mode, approved by the user, because its existing inline shift projection reads IEvent.Sequence. Marten 9's default QuickWithServerTimestamps does not assign that metadata before inline projection execution. All five PointOfSales tests pass.
- All nine disputed assertions pass in isolated copies with Ogooreck 0.8.2 and fail with 0.10.0 against unchanged business implementations. The older ThenThrows implementation returned successfully even when no exception occurred. The user subsequently approved the nine assertion corrections on 2026-11-06: the tests now assert the cancellation, charge-recorded, and checkout-failure events, and their names reflect the outcomes. All 153 affected-suite tests pass. No business methods were changed.
- Fresh-schema comparison: the complete pre-upgrade stack (Marten 8.22.0, xUnit adapter 2.8.2, Microsoft.NET.Test.Sdk 17.13.0) and the new stack each fail two of the three solved 05-GettingStateFromEvents.Marten tests when independent stores concurrently create one shared schema. The original stack fails on duplicate PostgreSQL namespace catalog entries; the new stack also exposes duplicate event-index creation. Isolated schemas pass all three tests. This is an existing first-run race, not proof that Marten 9 requires a new layout. The approved repository fix uses stable names: getting_state_mutable and getting_state_immutable for the exercise, and solved_getting_state_mutable, solved_getting_state_immutable_default_convention, and solved_getting_state_immutable_single_apply for the solutions. The three solved tests pass on first creation; SQL inspection confirms six retained events in each solved schema. The unsolved project builds without running its existing SkipCI exercises. After the schema change and before the approved assertion corrections, the main CI command was rerun: 735 passed, 9 failed, and 1 skipped, with only the nine assertion mismatches failing. The separate Marten CI suite passed again with 22 passed and 4 skipped.

The main build may emit ASPIRE010 because CLI bundle delegation uses the SDK default. The existing AppHost launch mode remains supported; see [Aspire's diagnostic documentation](https://aspire.dev/diagnostics/aspire010/). The unfinished entities workshop also emits an existing unread-parameter warning.

The independent regression test for the generic workshop aggregate can be run with Docker available:

```sh
dotnet test Workshops/IntroductionToEventSourcing/Solved/08-ApplicationLogic.Marten.Tests/08-ApplicationLogic.Marten.Tests.csproj --configuration Release --filter FullyQualifiedName~MartenAggregationTests
```
