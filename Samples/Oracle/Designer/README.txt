After installing this package you must follow next steps to run designer:

1. Install one of these packages to include WorkflowEnginene.NET persistance provider in your application:

WorkflowEngine.NET Provider for MSSQL PM> Install-Package WorkflowEngine.NET-ProviderForMSSQL

WorkflowEngine.NET Provider for MongoDB PM> Install-Package WorkflowEngine.NET-ProviderForMongoDB

WorkflowEngine.NET Provider for RavenDB PM> Install-Package WorkflowEngine.NET-ProviderForRavenDB

2. Initialize WorkflowEnginene.NET runtime in getRuntime property in DesignerController (DesignerController.cs)

Instructions is here: http://workflowenginenet.com/Documentation/Detail/howtoconnect

3. Run your application and open /Designer - page


