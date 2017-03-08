/*
 * WorkflowEngine.NET
 * https://workflowengine.io
 */
 
 WorkflowEngine.NET - component that adds workflow in your application. 
 It can be fully integrated into your application, or be in the form of a specific service (such as a web service).

Demo on-line: https://workflowengine.io/demo/designer

Please note. All samples was assembled with ASP.NET MVC 5.0. If you need samples wich was assembled with ASP.NET MVC 4.0 you could take them here
http://workflowenginenet.com/Cms_Data/Contents/WFE/Media/downloads/1.5.4/Samples.zip

 
1. MongoDB - Sample for MongoDB

Check connection string in WF.Sample\Configuration\AppSettings.config
<add key="Url" value="mongodb://localhost:27017"/>
<add key="Database" value="WorkflowEngineNET"/>

When you first start the application on an empty database, click on button "Generate data" (at the top right of this page)


2. RavenDB - Sample for RavenDB

Check connection string in WF.Sample\Configuration\AppSettings.config
<add key="Url" value="http://localhost:8090/"/>
<add key="Database" value="WorkflowEngineNET"/>

When you first start the application on an empty database, click on button "Generate data" (at the top right of this page)

3. MSSQL - Sample for MS SQL Server

Check connection string in WF.Sample\Configuration\ConnectionString.config
You can restore DB\db.bak to MS SQL Server or execute scripts.
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql (For MS SQL Server) or DB\CreatePersistenceObjectsForAzureSQL.sql (For AzureSQL)
2. DB\CreateObjects.sql
3. DB\FillData.sql

4. Oracle - Sample for Oracle

1. Execute Designer\SQL\CreatePersistenceObjects.sql
2. Check connection string in Designer\Web.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

5. MySQL - Sample for MySQL

1. Execute Designer\SQL\CreatePersistenceObjects.sql
2. Check connection string in Designer\Web.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

6. PostgreSQL - Sample for PostgreSQL

1. Execute Designer\SQL\CreatePersistenceObjects.sql
2. Check connection string in Designer\Web.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

7. Redis - Sample for Redis

Specify connection to your Redis server in Console\WorkflowInit.cs and in Designer\Controllers\DesignerController.cs To setup connection to your Redis server 
you need to configure ConnectionMultiplexer object and pass it to the Redis provider consctructor. You can read about ConnectionMultiplexer configuration here
https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md

8. Ignite - Sample for Apache Ignite

You can use SimpleNode console application as Ignite Server Node. Designer and Console projects are running in Client mode. To configure your node use following instructions
  var store = Ignition.TryGetIgnite() ?? Ignition.Start(IgniteProvider.GetDefaultIgniteConfiguration());
Then you need to pass store object to the Ignite provider constructor.

