using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class FieldCompatibility
    {
        private Dictionary<string, string[]> CompatibilityTable = new Dictionary<string, string[]>();

        public FieldCompatibility()
        {
            CompatibilityTable.Add("int", new string[] { "int", "bigint", "smallint", "tinyint", "bit" });
            CompatibilityTable.Add("binary", new string[] { "binary", "varbinary" });
            CompatibilityTable.Add("money", new string[] { "money", "smallmoney" });
            CompatibilityTable.Add("char", new string[] { "char", "nchar", "varchar", "nvarchar", "text", "ntext" });
            CompatibilityTable.Add("date", new string[] { "date", "datetime", "datetime2", "smalldatetime" });
            CompatibilityTable.Add("decimal", new string[] { "numeric", "decimal", "real", "float" });
        }

        public FieldInfo CreateCompatibleField(FieldInfo field, FieldInfo targetFiled)
        {
            FieldInfo result = new FieldInfo();
            result.Name = field.Name;
            result.Nullable = targetFiled.Nullable;
            result.IsPK = targetFiled.IsPK;
            // check nullability
            if (field.Nullable) result.Nullable = true;
            if (CompatibleTypes(field.Type, targetFiled.Type))
            {
                int size = Math.Max(field.Size, targetFiled.Size);
                int precision = Math.Max(field.Precision, targetFiled.Precision);

                if (field.Size > targetFiled.Size)
                    result.Type = field.Type;
                else result.Type = targetFiled.Type;

                if ((field.BaseType == targetFiled.BaseType) && (!result.Type.Contains("(max)")))
                    result.SetSize(size, precision);
            }
            else
                throw new IncompatibleTypesException("Incompatible Types:" + field.Type + " : " + targetFiled.Type);
            return result;
        }

        public bool CompatibleTypes(string type1, string type2)
        {
            type1 = type1.Split('(')[0];
            type2 = type2.Split('(')[0];
            foreach (var c in CompatibilityTable)
            {
                var fType1 = c.Value.FirstOrDefault(e => e == type1);
                if (fType1 != null)
                {
                    bool foundType = c.Value.FirstOrDefault(e => e == type2) != null;
                    if (foundType) return true;
                }
            }
            return false;
        }
    }
}
