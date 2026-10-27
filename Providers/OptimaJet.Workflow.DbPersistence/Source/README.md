# WorkflowEngine.NETCore-ProviderForMSSQL

![NuGet version](https://img.shields.io/nuget/v/WorkflowEngine.NETCore-ProviderForMSSQL)
![NuGet downloads](https://img.shields.io/nuget/dt/WorkflowEngine.NETCore-ProviderForMSSQL)

**WorkflowEngine.NETCore-ProviderForMSSQL is the SQL Server and Azure SQL persistence provider for Workflow Engine by Optimajet.** It stores schemes, process instances, parameters, timers and transition history in SQL Server and Azure SQL.

[Product page][product] · [Installation guide][install-guide] · [Documentation][docs] · [Release notes][release-notes]

## Install

```bash
dotnet add package WorkflowEngine.NETCore-ProviderForMSSQL
```

The package depends on `WorkflowEngine.NETCore-Core` and `WorkflowEngine.NETCore-Migrator`.

## Configure the runtime

```csharp
using System.Xml.Linq;
using OptimaJet.Workflow.DbPersistence;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.Migrator;

var connectionString = Environment.GetEnvironmentVariable("WORKFLOW_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set WORKFLOW_CONNECTION_STRING for your database.");

var provider = new MSSQLProvider(connectionString);
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

This provider targets .NET Standard 2.0.

The provider uses Microsoft.Data.SqlClient. Run migrations during controlled application startup before Workflow Runtime begins processing work.

For manual database rollout, use the versioned scripts distributed with Workflow Engine instead of calling `RunMigrations()` in the application.

## Licensing and support

Workflow Engine is commercial software. Workflow Engine Free can be used within the limits and attribution terms in the [EULA][eula]. A trial key is available for evaluating licensed capabilities.

- Technical questions: [support@optimajet.com](mailto:support@optimajet.com)
- Editions and evaluation: [hello@optimajet.com](mailto:hello@optimajet.com)

[product]: https://workflowengine.io/products/dotnet-workflow-engine/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerformssql&utm_content=project-website
[install-guide]: https://docs.workflowengine.io/get-started/framework-agnostic-install/
[docs]: https://docs.workflowengine.io/
[release-notes]: https://docs.workflowengine.io/release-notes/
[eula]: https://workflowengine.io/agreements/eula/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerformssql&utm_content=licensing#workflow-engine-free

[Source repository and issues](https://github.com/optimajet/WorkflowEngine.NET)
