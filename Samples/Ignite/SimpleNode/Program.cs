using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using OptimaJet.Workflow.Ignite;

namespace SimpleNode
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = Ignition.TryGetIgnite() ?? Ignition.Start(IgniteProvider.GetDefaultIgniteConfiguration());

            while (true)
            {
                var ln = Console.ReadLine();

                if (!string.IsNullOrEmpty(ln) && ln.Trim().Equals("c", StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }
        }
    }
}
