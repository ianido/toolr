using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class FieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsPK { get; set; }
        public bool Nullable { get; set; }
        public bool IsExtra { get; set; }
        public string Value { get; set; }
        public int Size {
            get
            {
                string type = Type.ToLower().Split('(')[0];
                string size = "0";
                if (Type.ToLower().Split('(').Length > 1)
                {
                    size = Type.ToLower().Split('(')[1].Split(',')[0].Trim(')');  // decimal(1,5) => decimal | 1,5) => 1 | 5)
                    if (size.ToLower() == "max") size = "4000";
                }

                switch (type)
                {
                    case "int": return 4;
                    case "bigint": return 8;
                    case "bynary": return int.Parse(size);
                    case "char": return int.Parse(size);
                    case "varchar": return int.Parse(size);
                    case "nvarchar": return int.Parse(size)*2;
                    case "date": return 3;
                    case "datetime": return 8;
                    case "datetimeoffset": return int.Parse(size); 
                    case "decimal": return int.Parse(size);
                    case "numeric": return int.Parse(size);
                    case "float": return 8;
                    case "money": return 8;
                    case "nchar": return int.Parse(size) * 2;
                    case "ntext": return int.MaxValue;
                    case "text": return int.MaxValue - 1;
                    case "smallint": return 2;
                    case "smalldatetime": return 4;
                    case "uniqueidentifier": return 16;
                    case "smallmoney": return 4;
                    case "time": return int.Parse(size);
                    case "timestamp": return 8;
                    case "tinyint": return 1;
                    default:
                        return 0;
                    //TODO: Probably this need to be changed according to new types.
                }
            }
        }

        public int Precision
        {
            get
            {
                string type = Type.ToLower().Split('(')[0];
                string size1 = "0";
                if (Type.ToLower().Split('(').Length > 1)
                {
                    if (Type.ToLower().Split('(')[1].Split(',').Length > 1)  // decimal(1,5) => decimal | 1,5) => 1 | 5)
                        size1 = Type.ToLower().Split('(')[1].Split(',')[1].Trim(')');
                }

                switch (type)
                {
                    case "decimal": return int.Parse(size1);
                    case "numeric": return int.Parse(size1);
                    default:
                        return 0;
                }
            }
        }

        public FieldInfo()
        {
            Type = "";
        }
        public string BaseType
        {
            get
            {
                return Type.Split('(')[0];
            }            
        }

        public void SetSize(int size, int precision)
        {
            var temp1 = Type.Split('(');
            if (temp1.Length > 1) // Means have size 
            {
                var temp2 = temp1[1].Split(',');
                if (temp2.Length > 1) // Means have precision
                    Type = temp1[0] + "(" + size + "," + precision + ")";
                else
                    Type = temp1[0] + "(" + size + ")";
            }            
        }

        public static bool operator ==(FieldInfo obj1, FieldInfo obj2)
        {
            return (obj1.Name == obj2.Name
                        && obj1.Type == obj2.Type
                        && obj1.Nullable == obj2.Nullable);
        }

        public static bool operator !=(FieldInfo obj1, FieldInfo obj2)
        {
            return !(obj1.Name == obj2.Name
                        && obj1.Type == obj2.Type
                        && obj1.Nullable == obj2.Nullable);
        }

        public override bool Equals(object obj)
        {
            return (Name == ((FieldInfo)obj).Name
                        && Type == ((FieldInfo)obj).Type
                        && Nullable == ((FieldInfo)obj).Nullable);
        }

        public override int GetHashCode()
        {
            var hashCode = -657939666;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + IsPK.GetHashCode();
            hashCode = hashCode * -1521134295 + Nullable.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + Precision.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BaseType);
            return hashCode;
        }
    }
}
