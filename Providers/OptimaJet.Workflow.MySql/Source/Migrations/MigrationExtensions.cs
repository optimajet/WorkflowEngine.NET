using FluentMigrator;

namespace OptimaJet.Workflow.MySQL.Migrations;

internal static class MigrationExtensions
{
    public static void EmbeddedScript(this Migration migration, string scriptName)
    {
        string embeddedSqlScriptName = $"OptimaJet.Workflow.MySQL.Scripts.{scriptName}";
        migration.Execute.EmbeddedScript(embeddedSqlScriptName);
    }
}
