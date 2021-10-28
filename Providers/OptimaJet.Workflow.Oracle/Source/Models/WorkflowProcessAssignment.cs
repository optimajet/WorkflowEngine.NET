using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using OptimaJet.Workflow.Core.Fault;
using OptimaJet.Workflow.Core.Helpers;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Oracle;
using OptimaJet.Workflow.Plugins;
using Oracle.ManagedDataAccess.Client;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessAssignment : DbObject<WorkflowProcessAssignment>
    {
        static WorkflowProcessAssignment()
        {
            DbTableName = "WorkflowProcessAssignment";
            DBColumnsStatic.AddRange(new[]
            {
                new ColumnInfo {Name = nameof(Id), IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(AssignmentCode)},
                new ColumnInfo {Name = nameof(ProcessId), Type = OracleDbType.Raw},
                new ColumnInfo {Name = nameof(Name)},
                new ColumnInfo {Name = nameof(Description)},
                new ColumnInfo {Name = nameof(StatusState)},
                new ColumnInfo {Name = nameof(IsActive), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(IsDeleted), Type = OracleDbType.Byte},
                new ColumnInfo {Name = nameof(DateCreation), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(DateStart), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(DateFinish), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(DeadlineToStart), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(DeadlineToComplete), Type = OracleDbType.TimeStamp},
                new ColumnInfo {Name = nameof(Executor)},
                new ColumnInfo {Name = nameof(Observers)},
                new ColumnInfo {Name = nameof(Tags)}
            });
        }
        
        public WorkflowProcessAssignment()
        {
            DBColumns = DBColumnsStatic;
        }
        
        public Assignment ConvertToAssignment()
        {
            return new Assignment()
            {
                AssignmentId = Id,
                AssignmentCode = AssignmentCode,
                Name = Name,
                ProcessId = ProcessId,
                StatusState = StatusState,
                IsDeleted = IsDeleted,
                IsActive = IsActive,
                DateCreation = DateCreation,
                DateFinish = DateFinish,
                DateStart = DateStart,
                DeadlineToStart =  DeadlineToStart,
                DeadlineToComplete = DeadlineToComplete,
                Description = Description,
                Executor = Executor,
                Tags = JsonConvert.DeserializeObject<List<string>>(Tags),
                Observers = JsonConvert.DeserializeObject<List<string>>(Observers)
            };
        }
        
        public static ColumnInfo GetColumnInfoByAssignmentProperty(string propertyName)
        {
            return  propertyName switch
            {
                nameof(Assignment.AssignmentId) => DBColumnsStatic.Find(c=>c.Name == nameof(Id)),
                nameof(Assignment.AssignmentCode) => DBColumnsStatic.Find(c=>c.Name == nameof(AssignmentCode)),
                nameof(Assignment.Name) => DBColumnsStatic.Find(c=>c.Name == nameof(Name)),
                nameof(Assignment.ProcessId) => DBColumnsStatic.Find(c=>c.Name == nameof(ProcessId)),
                nameof(Assignment.StatusState) => DBColumnsStatic.Find(c=>c.Name == nameof(StatusState)),
                nameof(Assignment.IsDeleted) => DBColumnsStatic.Find(c=>c.Name == nameof(IsDeleted)),
                nameof(Assignment.IsActive) => DBColumnsStatic.Find(c=>c.Name == nameof(IsActive)),
                nameof(Assignment.DateCreation) => DBColumnsStatic.Find(c=>c.Name == nameof(DateCreation)),
                nameof(Assignment.DateFinish) => DBColumnsStatic.Find(c=>c.Name == nameof(DateFinish)),
                nameof(Assignment.DateStart) => DBColumnsStatic.Find(c=>c.Name == nameof(DateStart)),
                nameof(Assignment.DeadlineToStart) => DBColumnsStatic.Find(c=>c.Name == nameof(DeadlineToStart)),
                nameof(Assignment.DeadlineToComplete) => DBColumnsStatic.Find(c=>c.Name == nameof(DeadlineToComplete)),
                nameof(Assignment.Description) => DBColumnsStatic.Find(c=>c.Name == nameof(Description)),
                nameof(Assignment.Executor) => DBColumnsStatic.Find(c=>c.Name == nameof(Executor)),
                nameof(Assignment.Tags) => DBColumnsStatic.Find(c=>c.Name == nameof(Tags)),
                nameof(Assignment.Observers) => DBColumnsStatic.Find(c=>c.Name == nameof(Observers)),
                _ => throw new Exception(string.Format("Column {0} is not exists", propertyName))
            };
        }

        public Guid Id { get; set; }
        public string AssignmentCode { get; set; }
        public string Name { get; set; }
        public Guid ProcessId { get; set; }
        public string Description { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateFinish { get; set; }
        public DateTime? DeadlineToStart { get; set; }
        public DateTime? DeadlineToComplete { get; set; }
        public string Executor { get; set; }
        public string Observers { get; set; }
        public string Tags { get; set; }
        public string StatusState { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        

        public override object GetValue(string key)
        {
            return key switch
            {
                nameof(Id) => Id.ToByteArray(),
                nameof(AssignmentCode) => AssignmentCode,
                nameof(ProcessId) => ProcessId.ToByteArray(),
                nameof(Description) => Description,
                nameof(DateCreation) => DateCreation,
                nameof(DateStart) => DateStart,
                nameof(DateFinish) => DateFinish,
                nameof(Name) => Name,
                nameof(DeadlineToStart) => DeadlineToStart,
                nameof(DeadlineToComplete) => DeadlineToComplete,
                nameof(Executor) => Executor,
                nameof(Observers) => Observers,
                nameof(Tags) => Tags,
                nameof(IsDeleted) => IsDeleted ? "1" : "0",
                nameof(IsActive) => IsActive ? "1" : "0",
                nameof(StatusState) => StatusState,
                _ => throw new Exception(string.Format("Column {0} is not exists", key))
            };
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case nameof(Id):
                    Id = new Guid((byte[])value);
                    break;
                case nameof(AssignmentCode):
                    AssignmentCode = (string) value;
                    break;
                case nameof(ProcessId):
                    ProcessId = new Guid((byte[])value);
                    break;
                case nameof(Name):
                    Name = (string) value;
                    break;
                case nameof(Description):
                    Description = (string) value;
                    break;
                case nameof(StatusState):
                    StatusState = (string) value;
                    break;
                case nameof(IsActive):
                    IsActive =  (string)value == "1";;
                    break;
                case nameof(IsDeleted):
                    IsDeleted =  (string)value == "1";;
                    break;
                case nameof(DateCreation):
                    DateCreation = (DateTime) value;
                    break;
                case nameof(DateStart):
                    DateStart = (DateTime?) value;
                    break;
                case nameof(DateFinish):
                    DateFinish = (DateTime?) value;
                    break;
                case nameof(DeadlineToStart):
                    DeadlineToStart = (DateTime?) value;
                    break;
                case nameof(DeadlineToComplete):
                    DeadlineToComplete = (DateTime?) value;
                    break;
                case nameof(Executor):
                    Executor = (string) value;
                    break;
                case nameof(Observers):
                    Observers = (string) value;
                    break;
                case nameof(Tags):
                    Tags = (string) value;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static async Task<int> GetAssignmentCountAsync(OracleConnection connection, List<FilterParameter> parameters )
        {
            string selectText = $"SELECT COUNT(*) FROM {ObjectName} ";

            var sqlParams = GetSqlParametersWithText(parameters, selectText, out string updatedSelectText);
            
            object result = await ExecuteCommandScalarAsync(connection, updatedSelectText, sqlParams).ConfigureAwait(false);
            
            result = (result == DBNull.Value) ? null : result;
            return Convert.ToInt32(result);
        }
        
        public static async Task<IEnumerable<WorkflowProcessAssignment>> SelectByFilterAsync(
            OracleConnection connection,
            List<FilterParameter> parameters ,
            List<(string parameterName,SortDirection sortDirection)> orderParameters = null,
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
                    orderParameters.Add((nameof(DateCreation),SortDirection.Desc));
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
        
        public static async Task<int> DeleteByProcessIdAsync(OracleConnection connection, Guid processId, OracleTransaction transaction = null)
        {
            var pProcessId = new OracleParameter("processId", OracleDbType.Raw, processId.ToByteArray(),
                ParameterDirection.Input);
            try
            {
                return await ExecuteCommandNonQueryAsync(connection, $"DELETE FROM {ObjectName} WHERE PROCESSID = :processId",  pProcessId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.RethrowAllowedIfRetrievable();
            }
        }
        
        private static OracleParameter[] GetSqlParametersWithText(List<FilterParameter> parameters, string selectText, out string updatedSelectText)
        {
            var sqlParameterlist = new List<OracleParameter>();
            var i = 0;
            foreach (var p in parameters)
            {
                int n = 0;
                var columnInfo = GetColumnInfoByAssignmentProperty(p.Name);

                if (p.ExpressionType == FilterExpressionType.In)
                {
                    foreach (var param in (IEnumerable) p.Value)
                    {
                        if (columnInfo.Type == OracleDbType.Raw)
                        {
                            sqlParameterlist.Add(new OracleParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ((Guid) param).ToByteArray()});
                        }
                        else
                        {
                            sqlParameterlist.Add(new OracleParameter($"parameter{i}_{n}", columnInfo.Type) {Value = param});
                        }
                        n++;
                    }
                }
                else
                {
                    if (columnInfo.Type == OracleDbType.Raw)
                    {
                        sqlParameterlist.Add(new OracleParameter($"parameter{i}_{n}", columnInfo.Type) {Value = ((Guid) p.Value).ToByteArray()});
                    }
                    else
                    {
                        sqlParameterlist.Add(new OracleParameter($"parameter{i}_{n}", columnInfo.Type) {Value = p.Value});
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
            var result = "";
            var parameterName = "";
            var expression = "";
            
            switch (filterParameter.ExpressionType)
            {
                case FilterExpressionType.Equal:
                    parameterName = $":parameter{i}_{n}";
                    expression = " = ";
                    break;
                case FilterExpressionType.In:
                    var parameterList = new List<string>();
                    for (int j = 0; j < n; j++) {
                        parameterList.Add($":parameter{i}_{j}");
                    }
                    parameterName = $" ( {String.Join(",", parameterList)} ) ";
                    expression = " IN ";
                    break;
                case FilterExpressionType.Like:
                    parameterName = $" CONCAT(CONCAT('%\"', :parameter{i}_{n}) , '\"%' ) ";
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
        
        private static string GetOrderParameters(List<(string parameterName,SortDirection sortDirection)> orderParameters)
        {
            string result = String.Join(", ",
                orderParameters.Select(x => $"{x.parameterName} {x.sortDirection.UpperName()}"));
            return result;
        }
    }
}
