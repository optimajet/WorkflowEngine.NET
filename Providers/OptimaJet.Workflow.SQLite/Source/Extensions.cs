using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.SQLite
{
    public static class Extensions
    {
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
        
        private static bool CanRepeatQuery(this Exception exception)
        {
            return exception.IsDeadLockException();
        }
        
        private static bool IsDeadLockException(this Exception exception)
        {
            const int errorCodeBusy = 5;
            return exception switch
            {
                SqliteException {ErrorCode: errorCodeBusy} sqLiteException => true, //DeadLock exception
                _ => false
            };
        }
    }
}
