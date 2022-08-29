using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Persistence;
using Oracle.ManagedDataAccess.Client;

namespace OptimaJet.Workflow.Oracle
{
    public class ColumnInfo
    {
        public string Name;
        public OracleDbType Type = OracleDbType.NVarchar2;
        public bool IsKey = false;
        public int Size = 256;
        public bool IsVirtual = false;
    }

    public class DbObject<TEntity> where TEntity : IEntity, new()
    {
        public DbObject(string schemaName, string dbTableName, int commandTimeout)
        {
            SchemaName = schemaName;
            DbTableName = dbTableName;
            CommandTimeout = commandTimeout;
        }

        public int CommandTimeout { get; }
        public string SchemaName { get; }
        public string DbTableName { get; }

        public string ObjectName =>
            !String.IsNullOrWhiteSpace(SchemaName)
                ? $"{SchemaName}.{DbTableName}"
                : $"{DbTableName}";

        // ReSharper disable once StaticMemberInGenericType
        public static List<ColumnInfo> DBColumnsStatic = new List<ColumnInfo>();

        public List<ColumnInfo> DBColumns = new List<ColumnInfo>();

        #region Command Insert/Update/Delete/Commit

        public async Task<int> InsertAsync(OracleConnection connection, TEntity entity, OracleTransaction transaction = null)
        {
            string command = $"INSERT INTO {ObjectName} " +
                             $"({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => c.Name.ToUpperInvariant()))}) " +
                             $"VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => ":" + c.Name))})";

            OracleParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }
        
        public async Task<int> UpsertAsync(OracleConnection connection, TEntity entity, OracleTransaction transaction = null)
        {
            string command =
                "BEGIN LOOP BEGIN " +
                $"MERGE INTO {ObjectName} " + 
                $"USING dual ON ({String.Join(",", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name))}) " +
                "WHEN NOT MATCHED THEN " +
                $"INSERT ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => c.Name.ToUpperInvariant()))}) " +
                $"VALUES ({String.Join(",", DBColumns.Where(c => !c.IsVirtual).Select(c => ":" + c.Name))}) " +
                "WHEN MATCHED THEN UPDATE SET " +
                $"{String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name))}; " +
                "EXIT; -- success? -> exit loop " + Environment.NewLine + 
                "EXCEPTION " +
                "WHEN NO_DATA_FOUND THEN " +
                "NULL; -- exception? -> no op, i.e. continue looping " + Environment.NewLine +
                " WHEN DUP_VAL_ON_INDEX THEN " +
                "NULL; -- exception? -> no op, i.e. continue looping " + Environment.NewLine +
                "END; END LOOP; END;";
            
            var parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters)
                .ConfigureAwait(false);
        }

        public virtual async Task<int> UpdateAsync(OracleConnection connection, TEntity entity, OracleTransaction transaction = null)
        {
            string command = $@"UPDATE {ObjectName} SET " +
                             $"{String.Join(",", DBColumns.Where(c => !c.IsVirtual).Where(c => !c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name))} " +
                             $"WHERE {String.Join(" AND ", DBColumns.Where(c => c.IsKey).Select(c => c.Name.ToUpperInvariant() + " = :" + c.Name))}";

            OracleParameter[] parameters = DBColumns.Select(c => CreateParameter(entity, c)).ToArray();

            return await ExecuteCommandNonQueryAsync(connection, command, transaction, parameters).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllAsync(OracleConnection connection)
        {
            return await SelectAsync(connection, $"SELECT * FROM {ObjectName}").ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAllWithPagingAsync(OracleConnection connection,
            List<(string parameterName, SortDirection sortDirection)> orderParameters,
            Paging paging)
        {
            orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();

            string pagingText = String.Empty;
            string orderText = String.Empty;

            if (paging != null)
            {
                //default sort for paging
                if (orderParameters.Count < 1)
                {
                    orderParameters.Add((GetKeyOrFirstColumn(), SortDirection.Asc));
                }

                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            string selectText = $"SELECT * FROM {ObjectName}{orderText}{pagingText}";

            return await SelectAsync(connection, selectText).ConfigureAwait(false);
        }

        public async Task<int> GetCountAsync(OracleConnection connection)
        {
            var result = await ExecuteCommandScalarAsync(connection, $"SELECT COUNT(*) FROM {ObjectName}")
                .ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<int> GetCountByAsync<TSelect>(OracleConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, OracleTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"p_{propertyName}";
            string selectText = String.Format($"SELECT COUNT(*) FROM {ObjectName} WHERE {propertyName?.ToUpperInvariant()} = :{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new OracleParameter(paramName, column.Type, ParameterDirection.Input) {Value = ToDbValue(value, column.Type)};

            var result = await ExecuteCommandScalarAsync(connection, selectText, transaction, param).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<TEntity[]> SelectByAsync<TSelect>(OracleConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"p_{propertyName}";

            string selectText = String.Format($"SELECT * FROM {ObjectName} WHERE {propertyName?.ToUpperInvariant()} = :{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new OracleParameter(paramName, column.Type, ParameterDirection.Input) {Value = ToDbValue(value, column.Type)};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<int> DeleteByAsync<TSelect>(OracleConnection connection, Expression<Func<TEntity, TSelect>> selectorLambda,
            TSelect value, OracleTransaction transaction = null)
        {
            string propertyName = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string paramName = $"p_{propertyName}";
            string selectText = String.Format($"Delete FROM {ObjectName} WHERE {propertyName?.ToUpperInvariant()} = :{paramName}");

            var column = DBColumns.Find(x => x.Name == propertyName);
            var param = new OracleParameter(paramName, column.Type, ParameterDirection.Input) {Value = ToDbValue(value, column.Type)};

            return await ExecuteCommandNonQueryAsync(connection, selectText, transaction, param).ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectByWithPagingAsync<TSelect, TSort>(OracleConnection connection,
            Expression<Func<TEntity, TSelect>> selectorLambda, TSelect value, Expression<Func<TEntity, TSort>> sortLambda,
            SortDirection sortDirection, Paging paging)
        {
            string selectorProperty = (selectorLambda.Body as MemberExpression)?.Member.Name;
            string sortProperty = (sortLambda.Body as MemberExpression)?.Member.Name;

            string paramName = $"p_{selectorProperty}";

            string pagingText = String.Empty;

            if (paging != null)
            {
                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            string selectText =
                String.Format(
                    $"SELECT * FROM {ObjectName} WHERE {selectorProperty?.ToUpperInvariant()} = :{paramName} ORDER BY {sortProperty?.ToUpperInvariant()} {sortDirection.UpperName().ToUpperInvariant()}{pagingText}");

            var column = DBColumns.Find(x => x.Name == selectorProperty);
            var param = new OracleParameter(paramName, column.Type, ParameterDirection.Input) {Value = ToDbValue(value, column.Type)};

            return await SelectAsync(connection, selectText, param).ConfigureAwait(false);
        }

        public async Task<TEntity> SelectByKeyAsync(OracleConnection connection, object id)
        {
            ColumnInfo key = DBColumns.FirstOrDefault(c => c.IsKey);
            if (key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            string selectText = $"SELECT * FROM {ObjectName} WHERE {key.Name.ToUpperInvariant()} = :p_id";
            OracleParameter[] parameters = new[] {new OracleParameter("p_id", key.Type, ToDbValue(id, key.Type), ParameterDirection.Input)};

            return (await SelectAsync(connection, selectText, parameters).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<int> DeleteAsync(OracleConnection connection, object id, OracleTransaction transaction = null)
        {
            ColumnInfo key = DBColumns.FirstOrDefault(c => c.IsKey);

            if (key == null)
            {
                throw new Exception($"Key for table {ObjectName} isn't defined.");
            }

            return await ExecuteCommandNonQueryAsync(connection,
                    $"DELETE FROM {ObjectName} WHERE {key.Name.ToUpperInvariant()} = :p_id",
                    transaction,
                    new OracleParameter(
                        "p_id",
                        key.Type,
                        ToDbValue(id, key.Type),
                        ParameterDirection.Input
                    ))
                .ConfigureAwait(false);
        }

        public async Task<int> ExecuteCommandNonQueryAsync(OracleConnection connection, string commandText,
            params OracleParameter[] parameters)
        {
            return await ExecuteCommandNonQueryAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }
        
        public async Task<int> ExecuteCommandNonQueryAsync(OracleConnection connection, string commandText,
            OracleTransaction transaction = null, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.BindByName = true;
            command.Parameters.AddRange(parameters);
            int cnt = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return cnt;
        }

        public async Task<object> ExecuteCommandScalarAsync(OracleConnection connection, string commandText,
            params OracleParameter[] parameters)
        {
            return await ExecuteCommandScalarAsync(connection, commandText, null, parameters).ConfigureAwait(false);
        }
        
        public async Task<object> ExecuteCommandScalarAsync(OracleConnection connection, string commandText,
            OracleTransaction transaction, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            
            command.CommandTimeout = CommandTimeout;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            command.BindByName = true;
            command.Parameters.AddRange(parameters);
            return await command.ExecuteScalarAsync().ConfigureAwait(false);
        }

        public async Task<TEntity[]> SelectAsync(OracleConnection connection, string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();

            command.Connection = connection;
            command.CommandTimeout = CommandTimeout;

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = commandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities

            command.BindByName = true;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);

            var entities = new List<TEntity>();

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var entity = new TEntity();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string name = reader.GetName(i);
                    ColumnInfo column = DBColumns.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (column != null)
                    {
                        var value = await reader.IsDBNullAsync(i).ConfigureAwait(false) ? null : reader.GetValue(i);
                        entity.SetValue(ColumnToPropertyName(column.Name), FromDbValue(value, column.Type));
                    }
                }

                entities.Add(entity);
            }

            return entities.ToArray();
        }

        public async Task<List<Dictionary<string, object>>> SelectAsDictionaryAsync(OracleConnection connection,
            string commandText, params OracleParameter[] parameters)
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            using OracleCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandTimeout = CommandTimeout;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            command.CommandText = commandText;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.BindByName = true;
            command.CommandType = CommandType.Text;
            command.Parameters.AddRange(parameters);
            var res = new List<Dictionary<string, object>>();
            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var item = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    item.Add(name, reader.IsDBNull(i) ? null : reader.GetValue(i));
                }

                res.Add(item);
            }

            return res;
        }

        #endregion

        public async Task<int> InsertAllAsync(OracleConnection connection, TEntity[] entities, OracleTransaction transaction = null)
        {
            if (entities.Length < 1)
            {
                return 0;
            }

            foreach (TEntity entity in entities)
            {
                await InsertAsync(connection, entity, transaction).ConfigureAwait(false);
            }

            return entities.Length;
        }

        public OracleParameter CreateParameter(TEntity entity, ColumnInfo column, string namePrefix = "")
        {
            return new OracleParameter(
                namePrefix + column.Name,
                column.Type,
                ParameterDirection.Input
            ) {Value = ToDbValue(entity.GetValue(ColumnToPropertyName(column.Name)), column.Type)};
        }

        protected static object ToDbValue<TValue>(TValue value, OracleDbType type)
        {
            return type switch
            {
                OracleDbType.Raw => value is Guid guid ? (object)guid.ToByteArray() : value,
                OracleDbType.Int16 => value is byte b ? (object)(short)b : value,
                OracleDbType.Byte => value is bool b ? (object)(b ? "1" : "0") : value,
                OracleDbType.Decimal => value is long l ? (object)(decimal)l : value,
                _ => value
            };
        }

        protected static object FromDbValue<TValue>(TValue dbValue, OracleDbType type)
        {
            return type switch
            {
                OracleDbType.Raw => dbValue is byte[] bytes ? (object)new Guid(bytes) : dbValue,
                OracleDbType.Int16 => dbValue is short sh ? (object)(byte)sh : dbValue,
                OracleDbType.Byte => dbValue is string s ? (object)(s == "1") : dbValue,
                OracleDbType.Decimal => dbValue is decimal d ? (object)(long)d : dbValue,
                _ => dbValue
            };
        }

        private static string ColumnToPropertyName(string name)
        {
            return name switch
            {
                "LOCKFLAG" => "Lock",
                _ => name
            };
        }

        private static string GetOrderParameters(List<(string parameterName, SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"{x.parameterName.ToUpperInvariant()} {x.sortDirection.UpperName()}"));
            return result;
        }

        public string GetKeyOrFirstColumn()
        {
            ColumnInfo column = DBColumns.FirstOrDefault(c => c.IsKey) ?? DBColumns.First();
            return column.Name;
        }
    }
}
