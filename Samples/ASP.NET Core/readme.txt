/*
 * WorkflowEngine.NET
 * https://workflowengine.io
 */
 
 WorkflowEngine.NET - component that adds workflow in your application. 
 It can be fully integrated into your application, or be in the form of a specific service (such as a web service).

Demo on-line: https://workflowengine.io/demo/designer

Please note. All samples was assembled with NETCOREAPP 2.1. Please use VS2019.
 
1. MongoDB - Sample for MongoDB

Check connection string in WF.Sample\appsettings.json
"Url": "mongodb://localhost:27017/",
"Database": "wfsample"

2. MSSQL - Sample for MS SQL Server

Check connection string in WF.Sample\appsettings.json
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

3. Oracle - Sample for Oracle

Check connection string in WF.Sample\appsettings.json
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

4. MySQL - Sample for MySQL

Check connection string in WF.Sample\appsettings.json
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql

5. PostgreSQL - Sample for PostgreSQL

Check connection string in WF.Sample\appsettings.json
The order of execution of scripts:
1. DB\CreatePersistenceObjects.sql
2. DB\CreateObjects.sql
3. DB\FillData.sql


