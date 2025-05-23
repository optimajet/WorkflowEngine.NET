using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Entities;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.MySQL
{
    public class WorkflowProcessAssignment : DbObject<ProcessAssignmentEntity>
    {
        static WorkflowProcessAssignment()
        {
            DBColumnsStatic.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Id), IsKey = true, Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.AssignmentCode)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.ProcessId), Type = MySqlDbType.Binary},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Name)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Description)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.StatusState)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsActive), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.IsDeleted), Type = MySqlDbType.Bit},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateCreation), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateStart), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DateFinish), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToStart), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.DeadlineToComplete), Type = MySqlDbType.DateTime},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Executor)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Observers)},
                new ColumnInfo {Name = nameof(ProcessAssignmentEntity.Tags)}
            });
        }
        
        public WorkflowProcessAssignment(int commandTimeout) : base("workflowprocessassignment", commandTimeout)
        {
            DBColumns = DBColumnsStatic;
        }
        
        public Assignment ConvertToAssignment(ProcessAssignmentEntity assigment)
        {
            return new Assignment()
            {
                AssignmentId = assigment.Id,
                AssignmentCode = assigment.AssignmentCode,
                Name = assigment.Name,
                ProcessId = assigment.ProcessId,
                StatusState = assigment.StatusState,
                IsDeleted = assigment.IsDeleted,
                IsActive = assigment.IsActive,
                DateCreation = assigment.DateCreation,
                DateFinish = assigment.DateFinish,
                DateStart = assigment.DateStart,
                DeadlineToStart =  assigment.DeadlineToStart,
                DeadlineToComplete = assigment.DeadlineToComplete,
                Description = assigment.Description,
                Executor = assigment.Executor,
                Tags = JsonConvert.DeserializeObject<List<string>>(assigment.Tags),
                Observers = JsonConvert.DeserializeObject<List<string>>(assigment.Observers)
            };
        }
        
        public static ColumnInfo GetColumnInfoByAssignmentProperty(string propertyName)
        {
            return  propertyName switch
            {
                nameof(Assignment.AssignmentId) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Id)),
                nameof(Assignment.AssignmentCode) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.AssignmentCode)),
                nameof(Assignment.Name) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Name)),
                nameof(Assignment.ProcessId) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.ProcessId)),
                nameof(Assignment.StatusState) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.StatusState)),
                nameof(Assignment.IsDeleted) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.IsDeleted)),
                nameof(Assignment.IsActive) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.IsActive)),
                nameof(Assignment.DateCreation) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.DateCreation)),
                nameof(Assignment.DateFinish) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.DateFinish)),
                nameof(Assignment.DateStart) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.DateStart)),
                nameof(Assignment.DeadlineToStart) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.DeadlineToStart)),
                nameof(Assignment.DeadlineToComplete) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.DeadlineToComplete)),
                nameof(Assignment.Description) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Description)),
                nameof(Assignment.Executor) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Executor)),
                nameof(Assignment.Tags) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Tags)),
                nameof(Assignment.Observers) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessAssignmentEntity.Observers)),
                _ => throw new Exception(string.Format("Column {0} is not exists", propertyName))
            };
        }

        public async Task<int> GetAssignmentCountAsync(MySqlConnection connection, List<FilterParameter> parameters )
        {
            string selectText = $"SELECT COUNT(*) FROM {DbTableName} ";

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);
            
            object result = await ExecuteCommandScalarAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public async Task<IEnumerable<ProcessAssignmentEntity>> SelectByFilterAsync(
            MySqlConnection connection,
            List<FilterParameter> parameters ,
            List<(string parameterName,SortDirection sortDirection)> orderParameters = null,
            Paging paging = null)
        {
            orderParameters ??= new List<(string parameterName, SortDirection sortDirection)>();
            
            string selectText = $"SELECT * FROM {DbTableName} ";

            string pagingText = String.Empty;
            string orderText = String.Empty;
            
            if (paging != null)
            {
                //default sort for paging
                if (orderParameters.Count < 1)
                {
                    orderParameters.Add((nameof(ProcessAssignmentEntity.DateCreation),SortDirection.Desc));
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
        
        public async Task<int> DeleteByProcessIdAsync(MySqlConnection connection, Guid processId, MySqlTransaction transaction = null)
        {
            var pProcessId = new MySqlParameter("processId", MySqlDbType.Binary) {Value = processId.ToByteArray()};
            try
            {
                return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {DbTableName} WHERE `ProcessId` = @processId", transaction, pProcessId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }
        
        private static MySqlParameter[] GetSqlParametersWithText(List<FilterParameter> parameters, string selectText, out string updatedSelectText)
        {
            var sqlParameterlist = new List<MySqlParameter>();
            var i = 0;
            foreach (var p in parameters)
            {
                int n = 0;
                var columnInfo = GetColumnInfoByAssignmentProperty(p.Name);

                if (p.ExpressionType == FilterExpressionType.In)
                {
                    foreach (var param in (IEnumerable) p.Value)
                    {
                        if (columnInfo.Type == MySqlDbType.Binary)
                        {
                            sqlParameterlist.Add(new MySqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ((Guid)param).ToByteArray()});  
                        }
                        else
                        {
                            sqlParameterlist.Add(new MySqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = param});   
                        }
                        n++;
                    }
                }
                else
                {
                    if (columnInfo.Type == MySqlDbType.Binary)
                    {
                        sqlParameterlist.Add(new MySqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ((Guid)p.Value).ToByteArray()});
                    }
                    else
                    {
                        sqlParameterlist.Add(new MySqlParameter($"parameter{i}_{n}", columnInfo.Type) {Value = p.Value});
                    }
                }
                
                if (i == 0)
                {
                    selectText += $" WHERE {GetSqlExpression(p, i, n)} ";
                }
                else
                {
                    selectText += $" AND {GetSqlExpression(p, i , n)} ";
                }
                
                i++;
            }

            updatedSelectText = selectText;
            return sqlParameterlist.ToArray();
        }

        private static string GetSqlExpression(FilterParameter filterParameter, int i, int n )
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
                    for (int j = 0; j < n; j++) {
                        parameterList.Add($"@parameter{i}_{j}");
                    }
                    parameterName = $" ( {String.Join(",", parameterList)} ) ";
                    expression = " IN ";
                    break;
                case FilterExpressionType.Like:
                    parameterName = $" CONCAT('%\\\"', @parameter{i}_{n} , '\\\"%')  ";
                    expression = " LIKE ";
                    break;
            }

            if (filterParameter.InverseExpression)
            {
                result = $" NOT ( `{filterParameter.Name}` {expression} {parameterName} )";
            }
            else
            {
                result = $" `{filterParameter.Name}` {expression} {parameterName} ";
            }
            
            return result;
        }
        
        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"`{x.parameterName}` {x.sortDirection.UpperName()}"));
            return result;
        }
    }
}
