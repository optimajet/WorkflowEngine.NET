using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

[Migration(350)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.Precheck_WorkflowTenantIdAndTypeLength.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration350WorkflowTenantPrecheck : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(360)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowGlobalParameter_Type.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration360AlterColumnWorkflowGlobalParameterType : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(370)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowGlobalParameter_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration370WorkflowGlobalParameterTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(380)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropIndex_WorkflowGlobalParameter_TypeName_BeforeTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration380DropIndexWorkflowGlobalParameterTypeName : Migration
{
    public override void Up()
    {
        if (Schema.Table("workflowglobalparameter").Index("ix_workflowglobalparameter_type_name").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(390)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowGlobalParameter_TypeNameTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration390CreateIndexWorkflowGlobalParameterTypeNameTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowglobalparameter").Index("ix_workflowglobalparameter_type_name_tenantid").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(400)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration400WorkflowSchemeId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(410)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.Update_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration410UpdateWorkflowSchemeId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(420)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration420AlterColumnWorkflowSchemeId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(430)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropConstraint_WorkflowScheme_PrimaryKey.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration430DropConstraintWorkflowSchemePrimaryKey : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(440)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateConstraint_WorkflowScheme_PrimaryKey_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration440CreateConstraintWorkflowSchemePrimaryKeyId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(450)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowScheme_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration450WorkflowSchemeTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(460)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowScheme_CodeTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration460CreateIndexWorkflowSchemeCodeTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowscheme").Index("ix_workflowscheme_code_tenantid").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(470)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowForm_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration470WorkflowFormTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(480)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropIndex_WorkflowForm_NameVersion.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration480DropIndexWorkflowFormNameVersion : Migration
{
    public override void Up()
    {
        if (Schema.Table("workflowform").Index("ix_workflowform_name_version").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(490)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowForm_NameVersionTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration490CreateIndexWorkflowFormNameVersionTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowform").Index("ix_workflowform_name_version_tenantid").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(500)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowProcessScheme_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration500WorkflowProcessSchemeTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(510)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowApprovalHistory_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration510WorkflowApprovalHistoryTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(520)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowInbox_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration520WorkflowInboxTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(530)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AlterColumn_WorkflowProcessInstance_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration530AlterColumnWorkflowProcessInstanceTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(540)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowProcessInstancePersistence_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration540WorkflowProcessInstancePersistenceTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(550)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowProcessInstanceStatus_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration550WorkflowProcessInstanceStatusTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(560)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowProcessTimer_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration560WorkflowProcessTimerTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(570)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.AddColumn_WorkflowProcessTransitionHistory_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration570WorkflowProcessTransitionHistoryTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(580)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.Update_TenantId_Synchronization.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration580SynchronizeWorkflowTenantIds : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(590)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.DropIndex_WorkflowProcessScheme_SchemeCodeIsObsolete.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration590DropIndexWorkflowProcessSchemeSchemeCodeIsObsolete : Migration
{
    public override void Up()
    {
        if (Schema.Table("workflowprocessscheme").Index("ix_workflowprocessscheme_schemecode_isobsolete").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(600)]
[WorkflowEngineMigration("OptimaJet.Workflow.MySQL.Scripts.CreateIndex_WorkflowProcessScheme_SchemeTenantRootIsObsolete.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration600CreateIndexWorkflowProcessSchemeSchemeTenantRootIsObsolete : Migration
{
    public override void Up()
    {
        if (!Schema.Table("workflowprocessscheme").Index("ix_wfps_schemecode_tenantid_rootschemeid_isobsolete").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
