## WorkflowEngine.NET

[https://workflowengine.io](https://workflowengine.io)

Email: [support@optimajet.com](mailto:support@optimajet.com)

---

WorkflowEngine.NET - component that adds workflow in your application.
It can be fully integrated into your application, or be in the form of a specific service (such as a web service).

Demo on-line: https://demo.workflowengine.io/designer

1. **MongoDB** - Sample for MongoDB

   Check connection string in `WF.Sample\Configuration\AppSettings.config`
    ```xml
    <add key="Url" value="mongodb://localhost:27017"/>
    <add key="Database" value="WorkflowEngineNET"/>
    ```

2. **MSSQL** - Sample for MS SQL Server

   Check connection string in `WF.Sample\Configuration\ConnectionString.config`

   The order of execution of scripts:
    1. DB\CreatePersistenceObjects.sql
    2. DB\CreateObjects.sql
    3. DB\FillData.sql

3. **MySQL** - Sample for MySQL

   Check connection string in `WF.Sample\Configuration\ConnectionString.config`

   The order of execution of scripts:
    1. DB\CreatePersistenceObjects.sql
    2. DB\CreateObjects.sql
    3. DB\FillData.sql

4. **PostgreSQL** - Sample for PostgreSQL

   Check connection string in `WF.Sample\Configuration\ConnectionString.config`

   The order of execution of scripts:
    1. DB\CreatePersistenceObjects.sql
    2. DB\CreateObjects.sql
    3. DB\FillData.sql

5. **Redis** - Sample for Redis

   Specify connection to your Redis server in `WF.Sample\Configuration\AppSettings.config`
