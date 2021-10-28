using AbpAngularSample.Debugging;

namespace AbpAngularSample
{
    public class AbpAngularSampleConsts
    {
        public const string LocalizationSourceName = "AbpAngularSample";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "4e6c1937d4b14316bf78c3493b831306";
    }
}
