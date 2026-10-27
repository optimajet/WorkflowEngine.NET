# Workflow Engine by Optimajet ASP.NET Core samples

These samples embed Workflow Engine and Workflow Designer in ASP.NET Core applications targeting .NET 10.
The package references included in the archive identify the release used by each sample.

## Prerequisites

- .NET 10 SDK.
- SQL Server, PostgreSQL, MySQL, Oracle, or MongoDB for the corresponding sample directory.
- Database connection settings in the sample's `WF.Sample/appsettings.json`.

## Run a sample

For the SQL Server sample, from this directory:

```bash
dotnet restore MSSQL/WF.Sample.sln
dotnet run --project MSSQL/WF.Sample
```

Open the local address printed by ASP.NET Core. Use the Designer to open a scheme, then run a sample process.
For another database, use its directory: `MongoDB`, `MySQL`, `Oracle`, or `PostgreSQL`.
Relational samples apply database migrations during startup; grant the database account the permissions required
to create or update the schema. The MongoDB sample uses its configured URL and database name.

## Documentation and support

- [Install Workflow Engine](https://docs.workflowengine.io/get-started/install-workflow-engine/)
- [Designer integration](https://docs.workflowengine.io/get-started/integrate-the-designer/)
- [Release notes](https://docs.workflowengine.io/release-notes/)
- Technical questions: [support@optimajet.com](mailto:support@optimajet.com)

Workflow Engine is commercial software. See the included [EULA](LICENSE.md) for Workflow Engine Free limits
and the terms for licensed capabilities.
