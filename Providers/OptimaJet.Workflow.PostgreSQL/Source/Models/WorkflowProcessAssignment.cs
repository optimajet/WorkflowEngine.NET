using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessAssignment : DbObject<ProcessAssignmentEntity>
    {
        static WorkflowProcessAssignment()
        {
            DBColumnsStatic.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Id), IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.AssignmentCode)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.ProcessId), Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Description)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.StatusState)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsActive), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsDeleted), Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateCreation), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateStart), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateFinish), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToStart), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToComplete), Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Executor)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Observers)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Tags)}
            });
        }

        public WorkflowProcessAssignment(string schemaName, int commandTimeout) 
            : base(schemaName, "WorkflowProcessAssignment", commandTimeout)
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
        
        public async Task<int> GetAssignmentCountAsync(NpgsqlConnection connection, List<FilterParameter> parameters)
        {
            string selectText = $"SELECT COUNT(*) FROM {ObjectName} ";

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);

            object result = await ExecuteCommandScalarAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);

            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }

        public async Task<IEnumerable<ProcessAssignmentEntity>> SelectByFilterAsync(
            NpgsqlConnection connection,
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

                pagingText = $" OFFSET {paging.SkipCount()} ROWS FETCH NEXT {paging.PageSize} ROWS ONLY";
            }

            if (orderParameters.Any())
            {
                orderText = $" ORDER BY {GetOrderParameters(orderParameters)}";
            }

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);
            updatedSelectText += $" {orderText} {pagingText}";

            return await SelectAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);
        }

        public async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId,
            NpgsqlTransaction transaction = null)
        {
            var pProcessId = new NpgsqlParameter("processId", NpgsqlDbType.Uuid) {Value = processId};
            try
            {
                return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} " + 
                                                                     $"WHERE \"{nameof(ProcessAssignmentEntity.ProcessId)}\" = @processId",
                    transaction, pProcessId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }

        private static NpgsqlParameter[] GetSqlParametersWithText(List<FilterParameter> parameters,
            string selectText,
            out string updatedSelectText)
        {
            var sqlParamsList = new List<NpgsqlParameter>();
            var i = 0;
            foreach (var p in parameters)
            {
                int n = 0;
                var columnInfo = GetColumnInfoByAssignmentProperty(p.Name);

                if (p.ExpressionType == FilterExpressionType.In)
                {
                    foreach (var param in (IEnumerable)p.Value)
                    {
                        sqlParamsList.Add(new NpgsqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = param});
                        n++;
                    }
                }
                else
                {
                    sqlParamsList.Add(new NpgsqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = p.Value});
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
                    parameterName = $" CONCAT( '%\"', @parameter{i}_{n}, '\"%' ) ";
                    expression = " LIKE ";
                    break;
            }

            if (filterParameter.InverseExpression)
            {
                result = $" NOT ( \"{filterParameter.Name}\" {expression} {parameterName} )";
            }
            else
            {
                result = $" \"{filterParameter.Name}\" {expression} {parameterName} ";
            }

            return result;
        }

        private static string GetOrderParameters(List<(string parameterName, SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"\"{x.parameterName}\" {x.sortDirection.UpperName()}"));
            return result;
        }
    }
}
