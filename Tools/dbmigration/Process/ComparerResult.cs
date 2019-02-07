using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class ComparerResult
    {
        public string sourceDB  { get; set; }
        public string targetDB { get; set; }
        public HashSet<string> NewSchemas { get; set; }
        public List<ComparerTableResult> NewTables { get; set; }
        public List<ComparerTableResult> RemovedTables { get; set; }
        public List<ComparerTableResult> ModifiedTables { get; set; }
        public List<ComparerTableResult> UntouchedTables { get; set; }

        public ComparerResult()
        {
            NewTables = new List<ComparerTableResult>();
            RemovedTables = new List<ComparerTableResult>();
            ModifiedTables = new List<ComparerTableResult>();
            UntouchedTables = new List<ComparerTableResult>();
            NewSchemas = new HashSet<string>();
        }
    }

    public class ComparerTableResult
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string FullName { get { return "[" + Schema + "].[" + Name + "]"; } }
        public List<string> Pks { get; set; }
        public List<FieldInfo> NewFields{ get; set; }
        public List<FieldInfo> RemovedFields { get; set; }
        public List<FieldInfo> UntouchedFields { get; set; }
        public List<FieldInfo> UpgradedFields { get; set; }
        public List<FieldInfo> ErrorFields { get; set; }

        public ComparerTableResult()
        {
            NewFields = new List<FieldInfo>();
            RemovedFields = new List<FieldInfo>();
            UpgradedFields = new List<FieldInfo>();
            ErrorFields = new List<FieldInfo>();
            UntouchedFields = new List<FieldInfo>();
        }
    }
}
