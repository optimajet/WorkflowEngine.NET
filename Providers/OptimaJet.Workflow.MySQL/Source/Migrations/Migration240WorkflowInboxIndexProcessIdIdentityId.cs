using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(240)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowInbox_ProcessId_IdentityId.sql")]
[ProductVersion("WorkflowEngine", "14.0.0")]
public class Migration240WorkflowInboxIndexProcessIdIdentityId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowinbox").Index("ix_workflowinbox_processId_identityId").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
