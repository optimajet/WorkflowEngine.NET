You can restore db.bak to MS SQL Server or execute scripts.

The order of execution of scripts:
1. CreatePersistenceObjects.sql (For MS SQL Server) or CreatePersistenceObjectsForAzureSQL.sql (For AzureSQL)
2. CreateObjects.sql
3. FillData.sql