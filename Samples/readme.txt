/*
 * Worflow Engine.NET 1.4.5
 * http://workflowenginenet.com
 */
 
 WorkflowEngine.NET - component that adds workflow in your application. 
 It can be fully integrated into your application, or be in the form of a specific service (such as a web service).

Demo on-line: http://demo.workflowenginenet.com
 
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
2. Check connection string in Designer\ConnectionString.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

5. MySQL - Sample for MySQL

1. Execute Designer\SQL\CreatePersistenceObjects.sql
2. Check connection string in Designer\ConnectionString.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

6. PostgreSQL - Sample for PostgreSQL

1. Execute Designer\SQL\CreatePersistenceObjects.sql
2. Check connection string in Designer\ConnectionString.config (connectionStrings section)
3. Check connection string in Console\App.config (connectionStrings section)

Additional Information: http://www.codeproject.com/Articles/865250/Workflow-Engine-NET

