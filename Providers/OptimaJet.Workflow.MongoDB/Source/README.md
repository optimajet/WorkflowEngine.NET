# WorkflowEngine.NETCore-ProviderForMongoDB

![NuGet version](https://img.shields.io/nuget/v/WorkflowEngine.NETCore-ProviderForMongoDB)
![NuGet downloads](https://img.shields.io/nuget/dt/WorkflowEngine.NETCore-ProviderForMongoDB)

**WorkflowEngine.NETCore-ProviderForMongoDB is the MongoDB persistence provider for Workflow Engine by Optimajet.** It stores schemes, process instances, parameters, timers and transition history in MongoDB. It can also be used with the MongoDB API for Azure Cosmos DB.

[Product page][product] · [Installation guide][install-guide] · [Documentation][docs] · [Release notes][release-notes]

## Install

```bash
dotnet add package WorkflowEngine.NETCore-ProviderForMongoDB
```

The package depends on `WorkflowEngine.NETCore-Core`.

## Configure the runtime

```csharp
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using OptimaJet.Workflow.MongoDB;

var connectionString = Environment.GetEnvironmentVariable("WORKFLOW_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Set WORKFLOW_CONNECTION_STRING for your database.");

var provider = new MongoDBProvider(connectionString);
var runtime = new WorkflowRuntime()
    .WithBuilder(new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache())
    .WithPersistenceProvider(provider)
    .Start();

// Stop this runtime when the application finishes processing workflows.
await runtime.ShutdownAsync();
```

Call `ShutdownAsync()` for each `WorkflowRuntime` during application shutdown.

## Database setup

The provider targets .NET Standard 2.0 and creates the required MongoDB collections and indexes at runtime. It does not use `WorkflowEngine.NETCore-Migrator` or relational SQL migration scripts.

Grant the database user permission to create collections and indexes when the application starts against a new database.

## Licensing and support

Workflow Engine is commercial software. Workflow Engine Free can be used within the limits and attribution terms in the [EULA][eula]. A trial key is available for evaluating licensed capabilities.

- Technical questions: [support@optimajet.com](mailto:support@optimajet.com)
- Editions and evaluation: [hello@optimajet.com](mailto:hello@optimajet.com)

[product]: https://workflowengine.io/products/dotnet-workflow-engine/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerformongodb&utm_content=project-website
[install-guide]: https://docs.workflowengine.io/get-started/framework-agnostic-install/
[docs]: https://docs.workflowengine.io/
[release-notes]: https://docs.workflowengine.io/release-notes/
[eula]: https://workflowengine.io/agreements/eula/?utm_source=nuget&utm_medium=referral&utm_campaign=workflowengine.netcore-providerformongodb&utm_content=licensing#workflow-engine-free

[Source repository and issues](https://github.com/optimajet/WorkflowEngine.NET)
