using System;
using System.Transactions;

namespace OptimaJet.Workflow.DbPersistence
{
    public static class PredefinedTransactionScopes
    {
        public static TransactionScope SerializableSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.Serializable,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public static TransactionScope ReadCommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.ReadCommitted,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public static TransactionScope RepeatableReadSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                            {
                                                IsolationLevel = IsolationLevel.RepeatableRead,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }

        public static TransactionScope ReadUncommittedSupressedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Suppress,
                                            new TransactionOptions
                                                {
                                                    IsolationLevel = IsolationLevel.ReadUncommitted,
                                                    Timeout = new TimeSpan(0, 10, 0)
                                                });
            }
        }

        public static TransactionScope SerializableScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Required,new TransactionOptions
                                            {
                                                IsolationLevel = IsolationLevel.Serializable,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }

        public static TransactionScope ReadCommittedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Required,
                                            new TransactionOptions
                                            {
                                                IsolationLevel = IsolationLevel.ReadCommitted,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }

        public static TransactionScope RepeatableReadScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Required,
                                            new TransactionOptions
                                            {
                                                IsolationLevel = IsolationLevel.RepeatableRead,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }

        public static TransactionScope ReadUncommittedScope
        {
            get
            {
                return new TransactionScope(TransactionScopeOption.Required,
                                            new TransactionOptions
                                            {
                                                IsolationLevel = IsolationLevel.ReadUncommitted,
                                                Timeout = new TimeSpan(0, 10, 0)
                                            });
            }
        }
    }
}