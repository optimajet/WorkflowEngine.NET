using Microsoft.EntityFrameworkCore.Storage;
using Ralms.EntityFrameworkCore.Oracle.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace WF.Sample.Oracle.Storage
{
    public class UnquotedOracleSqlGenerationHelper : OracleSqlGenerationHelper
    {
        public UnquotedOracleSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
          : base(dependencies)
        {
        }

        public override string DelimitIdentifier(string identifier)
        { 
            if(identifier.StartsWith("\"")) return identifier;
            return $"\"{EscapeIdentifier(identifier)}\"";
        }

        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            if (identifier.StartsWith("\"")) builder.Append(identifier);
            else
            {
                builder.Append('"');
                EscapeIdentifier(builder, identifier);
                builder.Append('"');
            }
            
        }
    }
}
