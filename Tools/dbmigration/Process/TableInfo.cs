using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class TableInfo
    {
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName { get { return "[" + Schema + "].[" + Name + "]"; }  }
        public List<FieldInfo> Fields { get; set; }
        public List<string> Pks {
            get
            {
                return Fields.Where(f => f.IsPK).Select(f => f.Name).ToList();
            }
        }

    }
}
