using FluentMigrator;

namespace OptimaJet.Workflow.Oracle.Migrations;

internal static class MigrationExtensions
{
    public static void EmbeddedScript(this Migration migration, string scriptName)
    {
        string embeddedSqlScriptName = $"OptimaJet.Workflow.Oracle.Scripts.{scriptName}";
        migration.Execute.EmbeddedScript(embeddedSqlScriptName);
    }
}
