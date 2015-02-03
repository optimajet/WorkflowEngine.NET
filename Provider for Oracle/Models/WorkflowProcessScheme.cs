using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimaJet.Workflow.Oracle
{
    public class WorkflowProcessScheme : DbObject<WorkflowProcessScheme>
    {
        public string DefiningParameters { get; set; }
        public string DefiningParametersHash { get; set; }
        public Guid Id { get; set; }
        public bool IsObsolete { get; set; }
        public string SchemeCode { get; set; }
        public string Scheme { get; set; }

        private const string _tableName = "WorkflowProcessScheme";

        public WorkflowProcessScheme()
            : base()
        {
            db_TableName = _tableName;
            db_Columns.AddRange(new ColumnInfo[]{
                new ColumnInfo(){Name="Id", IsKey = true, Type = OracleDbType.Raw},
                new ColumnInfo(){Name="DefiningParameters"},
                new ColumnInfo(){Name="DefiningParametersHash" },
                new ColumnInfo(){Name="IsObsolete", Type = OracleDbType.Byte },
                new ColumnInfo(){Name="SchemeCode" },
                new ColumnInfo(){Name="Scheme" }
            });
        }

        public override object GetValue(string key)
        {
            switch (key)
            {
                case "Id":
                    return Id.ToByteArray();
                case "DefiningParameters":
                    return DefiningParameters;
                case "DefiningParametersHash":
                    return DefiningParametersHash;
                case "IsObsolete":
                    return IsObsolete ? (string)"1": (string)"0";
                case "SchemeCode":
                    return SchemeCode;
                case "Scheme":
                    return Scheme;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = new Guid((byte[])value);
                    break;
                case "DefiningParameters":
                    DefiningParameters = value as string;
                    break;
                case "DefiningParametersHash":
                    DefiningParametersHash = value as string;
                    break;
                case "IsObsolete":
                    IsObsolete = (string)value == "1";
                    break;
                case "SchemeCode":
                    SchemeCode = value as string;
                    break;
                case "Scheme":
                    Scheme = value as string;
                    break;
                default:
                    throw new Exception(string.Format("Column {0} is not exists", key));
            }
        }

        public static WorkflowProcessScheme[] Select(OracleConnection connection, string schemeCode, string definingParametersHash, bool ignoreObsolete)
        {
            string selectText = string.Format("SELECT * FROM {0}  WHERE SchemeCode = :schemecode AND DefiningParametersHash = :dphash", _tableName);
            if (ignoreObsolete)
                selectText += " AND ISOBSOLETE = 0";

           return Select(connection, selectText, 
               new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                   new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash, ParameterDirection.Input));
        }

        public static int SetObsolete(OracleConnection connection, string schemeCode)
        {
            string command = string.Format("UPDATE {0} SET IsObsolete = 1 WHERE SchemeCode = :schemecode", _tableName);
            return ExecuteCommand(connection, command, 
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input));
        }

        public static int SetObsolete(OracleConnection connection, string schemeCode, string definingParametersHash)
        {
            string command = string.Format("UPDATE {0} SET IsObsolete = 1 WHERE SchemeCode = :schemecode AND DefiningParametersHash = :dphash", _tableName);

            return ExecuteCommand(connection, command, 
                new OracleParameter("schemecode", OracleDbType.NVarchar2, schemeCode, ParameterDirection.Input),
                    new OracleParameter("dphash", OracleDbType.NVarchar2, definingParametersHash, ParameterDirection.Input));
        }
    }
}
