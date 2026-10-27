# WorkflowEngine.NETCore-ProviderForSQLite

![NuGet version](https://img.shields.io/nuget/v/WorkflowEngine.NETCore-ProviderForSQLite)
![NuGet downloads](https://img.shields.io/nuget/dt/WorkflowEngine.NETCore-ProviderForSQLite)

**WorkflowEngine.NETCore-ProviderForSQLite is the SQLite persistence provider for evaluating Workflow Engine by Optimajet locally.** It stores schemes, process instances, parameters, timers and transition history in SQLite.

[Product page][product] · [Installation guide][install-guide] · [Documentation][docs] · [Release notes][release-notes]

## Install

```bash
dotnet add package WorkflowEngine.NETCore-ProviderForSQLite
```

The package depends on `WorkflowEngine.NETCore-Core` and `WorkflowEngine.NETCore-Migrator`.

## Configure the runtime

```csharp
using System.Xml.Linq;
using OptimaJet.Workflow.SQLite;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Migrator;

var connectionString = Environment.GetEnvironmentVariable("WORKFLOW_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set WORKFLOW_CONNECTION_STRING for your database.");

var provider = new SqliteProvider(connectionString);
var runtime = new WorkflowRuntime()
    .WithBuilder(new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache())
    .WithPersistenceProvider(provider)
    .RunMigrations()
    .Start();

// Stop this runtime when the application finishes processing workflows.
await runtime.ShutdownAsync();
```

Call `ShutdownAsync()` for each `WorkflowRuntime` during application shutdown.

## Compatibility and database setup

This provider targets .NET 8 and .NET 10.

SQLite support is intended for evaluation and self-contained examples. Choose SQL Server, PostgreSQL, MySQL, MongoDB, or Oracle for a production deployment.

For manual database rollout, use the versioned scripts distributed with Workflow Engine instead of calling `RunMigrations()` in the application.

## Licensing and support

Workflow Engine is commercial software. Workflow Engine Free can be used within the limits and attribution terms in the [EULA][eula]. A trial key is available for evaluating licensed capabilities.

- Technical questions: [support@optimajet.com](mailto:support@optimajet.com)
- Editions and evaluation: [hello@optimajet.com](mailto:hello@optimajet.com)

[product]: https://workflowengine.io/products/dotnet-workflow-engine/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerforsqlite&utm_content=project-website
[install-guide]: https://docs.workflowengine.io/get-started/framework-agnostic-install/
[docs]: https://docs.workflowengine.io/
[release-notes]: https://docs.workflowengine.io/release-notes/
[eula]: https://workflowengine.io/agreements/eula/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerforsqlite&utm_content=licensing#workflow-engine-free

[Source repository and issues](https://github.com/optimajet/WorkflowEngine.NET)
