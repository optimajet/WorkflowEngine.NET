using System;
using System.IO;
using Abp.Reflection.Extensions;

namespace AbpAngularSample
{
    /// <summary>
    /// Central point for application version.
    /// </summary>
    public class AppVersionHelper
    {
        /// <summary>
        /// Gets current version of the application.
        /// It's also shown in the web page.
        /// </summary>
        public const string Version = "5.5.0.0";

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public static DateTime ReleaseDate => LzyReleaseDate.Value;

        private static readonly Lazy<DateTime> LzyReleaseDate = new Lazy<DateTime>(() => new FileInfo(typeof(AppVersionHelper).GetAssembly().Location).LastWriteTime);
    }
}
