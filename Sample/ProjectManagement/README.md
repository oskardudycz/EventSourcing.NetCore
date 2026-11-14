[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# How to handle multiple commands in the same transaction

Sample for the article [How to handle multiple commands in the same transaction](https://event-driven.io/en/simple_transactional_command_orchestration/?utm_source=event_sourcing_net).

Take project management, for example, Jira-like software. We'd like to create a workspace for specific company departments where they can manage their projects. Our business wants to have the default project created during workspace creation.

_How to handle two commands_ or _how to change two aggregates in one transaction_ are one of the questions asked most often. The classical event-driven way is to use an [asynchronous process](https://event-driven.io/en/saga_process_manager_distributed_transactions/?utm_source=event_sourcing_net) together with the [outbox pattern](https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/?utm_source=event_sourcing_net). This sample shows a simpler option for cases where that's too much: orchestrating both commands in a single [Marten](https://martendb.io/) transaction.

The sample shows:

- separate business logic for [workspaces](./ProjectManagement/Workspaces/WorkspaceService.cs) and [projects](./ProjectManagement/Projects/ProjectService.cs), as they're separate things with different lifetimes and business rules,
- [CommandHandling.ComposeAsync](./ProjectManagement/Core/Marten/CommandHandling.cs) helper that appends events to multiple streams and saves them in one transaction,
- [WorkspaceCreationScenario](./ProjectManagement/Scenarios/WorkspaceCreation/WorkspaceCreationScenario.cs) that makes the orchestration explicit, and [WorkspaceCreationScenarioHandler](./ProjectManagement/Scenarios/WorkspaceCreation/WorkspaceCreationScenarioHandler.cs) that stores its result,
- [WorkspaceController](./ProjectManagement.Api/Controllers/WorkspaceController.cs) that separates command handling from data retrieval by returning `201 Created` with the `Location` header of the new workspace.

Before using this approach, make a sanity check: modifying too many entities in the same transaction leads to concurrency conflicts and deadlocks. If the scenario grows, you can replace it with a [Saga or another asynchronous coordination style](https://event-driven.io/en/saga_process_manager_distributed_transactions/?utm_source=event_sourcing_net).
