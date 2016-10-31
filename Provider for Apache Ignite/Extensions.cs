using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Apache.Ignite.Core;

namespace OptimaJet.Workflow.Ignite
{
    public static class IgniteConfigurationExtensions
    {
        public static string ToXml(this IgniteConfiguration cfg)
        {
            var sb = new StringBuilder();

            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var xmlWriter = XmlWriter.Create(sb, settings))
            {
                typeof(Ignition).Assembly
                    .GetType("Apache.Ignite.Core.Impl.Common.IgniteConfigurationXmlSerializer")
                    .GetMethod("Serialize")
                    .Invoke(null, new object[] { cfg, xmlWriter, "igniteConfiguration" });
            }

            return sb.ToString();
        }
    }
}
