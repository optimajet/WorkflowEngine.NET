using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Fault;

namespace OptimaJet.Workflow.SQLite
{
    /// <summary>
    /// Extension methods for SQLite exception handling.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts an exception to a <see cref="PersistenceProviderQueryException"/>,
        /// preserving retry information if the original exception is already a <see cref="PersistenceProviderQueryException"/>.
        /// </summary>
        /// <param name="exception">The exception to convert.</param>
        /// <param name="suppressRetry">If set to <c>true</c>, the resulting exception will not allow retry.</param>
        /// <returns>A <see cref="PersistenceProviderQueryException"/> wrapping the original exception.</returns>
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

        /// <summary>
        /// Determines whether the specified <see cref="PersistenceProviderQueryException"/> is caused by a duplicate key violation.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns><c>true</c> if the inner exception is a SQLite duplicate key error (error code 19); otherwise, <c>false</c>.</returns>
        public static bool IsDuplicateKeyException(this PersistenceProviderQueryException exception)
        {
            return exception.InnerException is SqliteException { SqliteErrorCode: 19 };
        }
    }
}
