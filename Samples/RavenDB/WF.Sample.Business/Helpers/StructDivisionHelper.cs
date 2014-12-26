using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web;
using WF.Sample.Business.Models;
using WF.Sample.Business.Workflow;

namespace WF.Sample.Business.Helpers
{
    public class StructDivisionHelper
    {
        public static object _lock = new object();

        private static List<StructDivision> _structDivisionAll = null;
        public static List<StructDivision> StructDivisionCache
        {
            get
            {
                if (_structDivisionAll == null)
                {
                    lock (_lock)
                    {
                        if (_structDivisionAll == null)
                        {
                            _structDivisionAll = GetAll();
                        }
                    }
                }

                return _structDivisionAll;
            }
        }

        public static List<StructDivision> GetAll()
        {
            using (var session = WorkflowInit.Provider.Store.OpenSession())
            {
                return session.Query<StructDivision>().ToList();
            }
        }
    }
}
