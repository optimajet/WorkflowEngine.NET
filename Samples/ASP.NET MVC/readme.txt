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

2. RavenDB - Sample for RavenDB

Check connection string in WF.Sample\Configuration\AppSettings.config
<add key="Url" value="http://localhost:8090/"/>
<add key="Database" value="WorkflowEngineNET"/>

3. MSSQL - Sample for MS SQL Server

Check connection string in WF.Sample\Configuration\ConnectionString.config
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

4. Oracle - Sample for Oracle

Check connection string in WF.Sample\Configuration\ConnectionString.config
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

5. MySQL - Sample for MySQL

Check connection string in WF.Sample\Configuration\ConnectionString.config
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

6. PostgreSQL - Sample for PostgreSQL

Check connection string in WF.Sample\Configuration\ConnectionString.config
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

7. Redis - Sample for Redis

Specify connection to your Redis server in  WF.Sample\Configuration\AppSettings.config

