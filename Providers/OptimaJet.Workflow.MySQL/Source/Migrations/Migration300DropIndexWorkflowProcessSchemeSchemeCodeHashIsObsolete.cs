using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(300)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropIndex_WorkflowProcessScheme_SchemeCodeHashIsObsolete.sql")]
public class Migration300DropIndexWorkflowProcessSchemeSchemeCodeHashIsObsolete : Migration
{
    public override void Up()
    {
        if (Schema.Table("workflowprocessscheme").Index("ix_workflowprocessscheme_schemecode_hash_isobsolete").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
