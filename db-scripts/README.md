# Workflow Engine Database Scripts

This directory contains database scripts for the supported database providers. 
Select the directory that matches your database
provider before applying any scripts.

Starting from version `13.0.0`, Workflow Engine supports automatic database
migrations. They can create a new database schema or upgrade an existing one.
For configuration details, see
[Database Versioning](https://docs.workflowengine.io/evaluate/workflow-engine-features/database-versioning/).

If you need to manage database changes manually instead, use the SQL scripts in
this directory as described below.

## Directory Structure

Each provider directory contains:

- `legacy/` - scripts for Workflow Engine versions earlier than `13.0.0`.
- `<version>/` - versioned scripts for Workflow Engine `13.0.0` and later.

The versioned scripts can be used both to create a new database schema and to
upgrade an existing schema.

## Execution Order

Apply versioned scripts in ascending order:

1. Process the version directories from the lowest applicable version to the
   target version. For a new database, start with `13.0.0`.
2. Within each version directory, process the scripts by the numeric version
   prefix at the beginning of the file name, from lowest to highest.

For example:

```text
13.0.0/10_...
13.0.0/15_...
14.0.0/20_...
20.0.0/30_...
```

Do not skip or reorder scripts that are required between the source and target
versions.

Before applying scripts to an existing database, create a backup and test the
upgrade procedure in a non-production environment.
