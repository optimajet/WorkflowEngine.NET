using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace Budget2.DAL
{
    public class Budget2DataContextService
    {
        public Budget2DataContext CreateContext ()
        {
            var context = new Budget2DataContext(ConfigurationManager.ConnectionStrings["default"].ConnectionString);
            context.CommandTimeout = 600;
            context.DeferredLoadingEnabled = true;
            return context;
        }

        public TransactionScope ReadCommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions()
                                                {
                                                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public TransactionScope ReadUncommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions()
                                            {
                                                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }
   
    }
}
