namespace WF.Sample.Business.Migrations
{
    public static class MigrationUtil
    {
        public static string GetEmbeddedPath(string fileName)
        {
            return $"WF.Sample.Business.Scripts.{fileName}";
        }
    }
}
