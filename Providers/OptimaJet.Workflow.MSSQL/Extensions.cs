using System;
using System.Data.SqlClient;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.DbPersistence
{
    public static class Extensions
    {
        public static bool IsDeadLockException(this Exception exception)
        {
            switch (exception)
            {
                case SqlException sqlException when sqlException.Number == 1205: //DeadLock exception
                    return true;
                default:
                    return false;
            }
        }
        
        public static bool CanRepeatQuery(this Exception exception)
        {
            return exception.IsDeadLockException();
        }

        public static PersistenceProviderQueryException ToQueryException(this Exception exception, bool suppressRetry = false)
        {
            if (exception is PersistenceProviderQueryException persistenceProviderQueryException)
            {
                return new PersistenceProviderQueryException(!suppressRetry && persistenceProviderQueryException.IsRetryAllowed, persistenceProviderQueryException.IsRetrievableError,
                    persistenceProviderQueryException);
            }

            bool canRepeatQuery = exception.CanRepeatQuery();
            return new PersistenceProviderQueryException(!suppressRetry && canRepeatQuery, canRepeatQuery, exception);
        }
    }
}
