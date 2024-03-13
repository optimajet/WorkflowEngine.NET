using System.Collections;
using System.Data;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.SQLite
{
    public class WorkflowProcessAssignment : DbObject<ProcessAssignmentEntity>
    {
        static WorkflowProcessAssignment()
        {
            DBColumnsStatic.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Id), IsKey = true, Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.AssignmentCode)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.ProcessId), Type = DbType.Guid},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Description)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.StatusState)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsActive), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsDeleted), Type = DbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateCreation), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateStart), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateFinish), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToStart), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToComplete), Type = DbType.DateTime2},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Executor)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Observers)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Tags)}
            });
        }

        public WorkflowProcessAssignment(string schemaName, int commandTimeout) : base(schemaName, "WorkflowProcessAssignment", commandTimeout)
        {
            DBColumns = DBColumnsStatic;
        }

        public Assignment ConvertToAssignment(ProcessAssignmentEntity entity)
        {
            return new Assignment()
            {
                AssignmentId = entity.Id,
                AssignmentCode = entity.AssignmentCode,
                Name = entity.Name,
                ProcessId = entity.ProcessId,
                StatusState = entity.StatusState,
                IsDeleted = entity.IsDeleted,
                IsActive = entity.IsActive,
                DateCreation = entity.DateCreation,
                DateFinish = entity.DateFinish,
                DateStart = entity.DateStart,
                DeadlineToStart = entity.DeadlineToStart,
                DeadlineToComplete = entity.DeadlineToComplete,
                Description = entity.Description,
                Executor = entity.Executor,
                Tags = JsonConvert.DeserializeObject<List<string>>(entity.Tags),
                Observers = JsonConvert.DeserializeObject<List<string>>(entity.Observers)
            };
        }

        public static ColumnInfo GetColumnInfoByAssignmentProperty(string propertyName)
        {
            return propertyName switch
            {
                nameof(Assignment.AssignmentId) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Id)),
                nameof(Assignment.AssignmentCode) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.AssignmentCode)),
                nameof(Assignment.Name) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Name)),
                nameof(Assignment.ProcessId) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.ProcessId)),
                nameof(Assignment.StatusState) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.StatusState)),
                nameof(Assignment.IsDeleted) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.IsDeleted)),
                nameof(Assignment.IsActive) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.IsActive)),
                nameof(Assignment.DateCreation) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.DateCreation)),
                nameof(Assignment.DateFinish) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.DateFinish)),
                nameof(Assignment.DateStart) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.DateStart)),
                nameof(Assignment.DeadlineToStart) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.DeadlineToStart)),
                nameof(Assignment.DeadlineToComplete) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.DeadlineToComplete)),
                nameof(Assignment.Description) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Description)),
                nameof(Assignment.Executor) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Executor)),
                nameof(Assignment.Tags) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Tags)),
                nameof(Assignment.Observers) => DBColumnsStatic.Find(c => c.Name == nameof(ProcessAssignmentEntity.Observers)),
                _ => throw new Exception($"Column {propertyName} is not exists")
            };
        }
        
        public async Task<int> GetAssignmentCountAsync(SqliteConnection connection, List<FilterParameter> parameters)
        {
            string selectText = $"SELECT COUNT(*) FROM {ObjectName} ";

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);

            object result = await ExecuteCommandScalarAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<ProcessAssignmentEntity>> SelectByFilterAsync(
            SqliteConnection connection,
            List<FilterParameter> parameters,
            List<(string parameterName, SortDirection sortDirection)> orderParameters = null,
            Paging paging = null)
        {
            orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();

            string selectText = $"SELECT * FROM {ObjectName} ";

            string pagingText = String.Empty;
            string orderText = String.Empty;

            if (paging != null)
            {
                //default sort for paging
                if (orderParameters.Count < 1)
                {
                    orderParameters.Add((nameof(ProcessAssignmentEntity.DateCreation), SortDirection.Desc));
                }

                pagingText = $" LIMIT {paging.PageSize} OFFSET {paging.SkipCount()}";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);
            updatedSelectText += $" {orderText} {pagingText}";

            return await SelectAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(SqliteConnection connection, Guid processId,
            SqliteTransaction transaction = null)
        {
            var pProcessId = new SqliteParameter("processId", DbType.String) {Value = ToDbValue(processId, DbType.Guid)};
            try
            {
                return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} " + 
                                                                     $"WHERE {nameof(ProcessAssignmentEntity.ProcessId)} = @processId",
                    transaction, pProcessId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }

        private static SqliteParameter[] GetSqlParametersWithText(List<FilterParameter> parameters,
            string selectText,
            out string updatedSelectText)
        {
            var sqlParamsList = new List<SqliteParameter>();
            var i = 0;
            foreach (var p in parameters)
            {
                int n = 0;
                var columnInfo = GetColumnInfoByAssignmentProperty(p.Name);

                if (p.ExpressionType == FilterExpressionType.In)
                {
                    foreach (var param in (IEnumerable)p.Value)
                    {
                        sqlParamsList.Add(new SqliteParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ToDbValue(param, columnInfo.Type)});
                        n++;
                    }
                }
                else
                {
                    sqlParamsList.Add(new SqliteParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ToDbValue(p.Value, columnInfo.Type)});
                }

                if (i == 0)
                {
                    selectText += $" WHERE {GetSqlExpression(p, i, n)} ";
                }
                else
                {
                    selectText += $" AND {GetSqlExpression(p, i, n)} ";
                }

                i++;
            }

            updatedSelectText = selectText;
            return sqlParamsList.ToArray();
        }

        private static string GetSqlExpression(FilterParameter filterParameter, int i, int n)
        {
            // ReSharper disable once RedundantAssignment
            var result = "";
            var parameterName = "";
            var expression = "";

            switch (filterParameter.ExpressionType)
            {
                case FilterExpressionType.Equal:
                    parameterName = $"@parameter{i}_{n}";
                    expression = " = ";
                    break;
                case FilterExpressionType.In:
                    var parameterList = new List<string>();
                    for (int j = 0; j < n; j++)
                    {
                        parameterList.Add($"@parameter{i}_{j}");
                    }

                    parameterName = $" ( {String.Join(",", parameterList)} ) ";
                    expression = " IN ";
                    break;
                case FilterExpressionType.Like:
                    parameterName = $" ( '%' || @parameter{i}_{n} || '%' ) ";
                    expression = " LIKE ";
                    break;
            }

            if (filterParameter.InverseExpression)
            {
                result = $" NOT ( {filterParameter.Name} {expression} {parameterName} )";
            }
            else
            {
                result = $" {filterParameter.Name} {expression} {parameterName} ";
            }

            return result;
        }

        private static string GetOrderParameters(List<(string parameterName, SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"{x.parameterName} {x.sortDirection.UpperName()}"));
            return result;
        }
    }
}
