using System;
using System.Collections.Generic;
using Apache.Ignite.Core.Binary;

namespace OptimaJet.Workflow.Ignite
{
    public class ColumnInfo
    {
        public string Name;
        public Type Type = typeof(string);
        public bool IsKey = false;
    }
    
    public class DbObject<T> : IBinarizable where T : DbObject<T>, new()
    {
        public static List<ColumnInfo> DbColumns = new List<ColumnInfo>();

        public virtual object GetValue(string key)
        {
            return null;
        }

        public virtual void SetValue(string key, object value)
        {
            
        }

        public static object ConvertToDbCompatibilityType(object obj)
        {
            return obj;
        }

        private static bool IsNullable<T1>(T1 obj)
        {
            if (obj == null) return true; 
            Type type = typeof (T1);
            if (!type.IsValueType) return true;
            if (Nullable.GetUnderlyingType(type) != null) return true; 
            return false;
        }

        public virtual void WriteBinary(IBinaryWriter writer)
        {
            foreach(var c in DbColumns)
            {
                if(c.Type == typeof(string))
                {
                    writer.WriteString(c.Name, (string)GetValue(c.Name));
                }
                else if(c.Type == typeof(Guid))
                {
                    writer.WriteGuid(c.Name, (Guid?)GetValue(c.Name));
                }
                else if (c.Type == typeof(byte))
                {
                    writer.WriteByte(c.Name, (byte)GetValue(c.Name));
                }
                else if (c.Type == typeof(bool))
                {
                    writer.WriteBoolean(c.Name, (bool)GetValue(c.Name));
                }
                else if (c.Type == typeof(DateTime))
                {
                    DateTime? dt = (DateTime?)GetValue(c.Name);
                    if (dt.HasValue)
                        dt = dt.Value.ToUniversalTime();
                    writer.WriteTimestamp(c.Name, dt);
                }
                else
                {
                    throw new Exception(string.Format("Unknow type {0}", c.Type));
                }
            }
        }

        public virtual void ReadBinary(IBinaryReader reader)
        {
            foreach (var c in DbColumns)
            {
                if (c.Type == typeof(string))
                {
                    SetValue(c.Name, reader.ReadString(c.Name));
                }
                else if (c.Type == typeof(Guid))
                {
                    Guid? tmp = reader.ReadGuid(c.Name);
                    if (tmp.HasValue)
                        SetValue(c.Name, tmp.Value);
                    SetValue(c.Name, tmp);
                }
                else if (c.Type == typeof(byte))
                {
                    SetValue(c.Name, reader.ReadByte(c.Name));
                }
                else if (c.Type == typeof(bool))
                {
                    SetValue(c.Name, reader.ReadBoolean(c.Name));
                }
                else if (c.Type == typeof(DateTime))
                {
                    DateTime? tmp = reader.ReadTimestamp(c.Name);
                    if (tmp.HasValue)
                        SetValue(c.Name, tmp.Value);
                    SetValue(c.Name, tmp);
                }
                else
                {
                    throw new Exception(string.Format("Unknow type {0}", c.Type));
                }
            }
        }
    }
}
