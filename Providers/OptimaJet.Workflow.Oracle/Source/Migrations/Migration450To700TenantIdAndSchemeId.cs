using FluentMigrator;
using OptimaJet.Workflow.Migrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

[Migration(450)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.Precheck_WorkflowTenantIdAndTypeLength.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration450WorkflowTenantPrecheck : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AlterColumn_WorkflowGlobalParameter_Type.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration460AlterColumnWorkflowGlobalParameterType : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(470)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowGlobalParameter_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration470WorkflowGlobalParameterTenantId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropIndex_WorkflowGlobalParameter_TypeName_BeforeTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration480DropIndexWorkflowGlobalParameterTypeName : Migration
{
    public override void Up()
    {
        if (Schema.Table("WORKFLOWGLOBALPARAMETER").Index("IDX_WORKFLOWGLOBALPARAMETER_TY").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(490)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowGlobalParameter_TypeNameTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration490CreateIndexWorkflowGlobalParameterTypeNameTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWGLOBALPARAMETER").Index("IDX_WORKFLOWGLOBALPARAMETER_TYPE_NAME_TENANTID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(500)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration500WorkflowSchemeId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.Update_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration510UpdateWorkflowSchemeId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AlterColumn_WorkflowScheme_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration520AlterColumnWorkflowSchemeId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropConstraint_WorkflowScheme_PrimaryKey.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration530DropConstraintWorkflowSchemePrimaryKey : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateConstraint_WorkflowScheme_PrimaryKey_Id.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration540CreateConstraintWorkflowSchemePrimaryKeyId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowScheme_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration550WorkflowSchemeTenantId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowScheme_CodeTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration560CreateIndexWorkflowSchemeCodeTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWSCHEME").Index("IDX_WORKFLOWSCHEME_CODE_TENANTID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(570)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowForm_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration570WorkflowFormTenantId : Migration
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
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropConstraint_WorkflowForm_NameVersion.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration580DropConstraintWorkflowFormNameVersion : Migration
{
    public override void Up()
    {
        if (Schema.Table("WORKFLOWFORM").Constraint("UQ_WORKFLOWFORM_NAME_VERSION").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(590)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowForm_NameVersionTenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration590CreateIndexWorkflowFormNameVersionTenantId : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWFORM").Index("UQ_WORKFLOWFORM_NAME_VERSION_TENANTID").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(600)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowProcessScheme_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration600WorkflowProcessSchemeTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(610)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowApprovalHistory_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration610WorkflowApprovalHistoryTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(620)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowInbox_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration620WorkflowInboxTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(630)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AlterColumn_WorkflowProcessInstance_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration630AlterColumnWorkflowProcessInstanceTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(640)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowProcessInstanceP_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration640WorkflowProcessInstancePTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(650)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowProcessInstances_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration650WorkflowProcessInstancesTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(660)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowProcessTimer_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration660WorkflowProcessTimerTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(670)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.AddColumn_WorkflowProcessTransition_TenantId.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration670WorkflowProcessTransitionTenantId : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(680)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.Update_TenantId_Synchronization.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration680SynchronizeWorkflowTenantIds : Migration
{
    public override void Up()
    {
        this.EmbeddedScript();
    }

    public override void Down()
    {
    }
}

[Migration(690)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.DropIndex_WorkflowProcessScheme_SchemeCodeHashIsObsolete.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration690DropIndexWorkflowProcessSchemeSchemeCodeIsObsolete : Migration
{
    public override void Up()
    {
        if (Schema.Table("WORKFLOWPROCESSSCHEME").Index("IDX_WORKFLOWPROCESSSCHEME_SCHE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}

[Migration(700)]
[WorkflowEngineMigration("OptimaJet.Workflow.Oracle.Scripts.CreateIndex_WorkflowProcessScheme_SchemeTenantRootIsObsolete.sql")]
[ProductVersion("WorkflowEngine", "22.0.0")]
public class Migration700CreateIndexWorkflowProcessSchemeSchemeTenantRootIsObsolete : Migration
{
    public override void Up()
    {
        if (!Schema.Table("WORKFLOWPROCESSSCHEME").Index("IDX_WORKFLOWPROCESSSCHEME_SCHEMECODE_TENANTID_ROOTSCHEMEID_ISOBSOLETE").Exists())
        {
            this.EmbeddedScript();
        }
    }

    public override void Down()
    {
    }
}
