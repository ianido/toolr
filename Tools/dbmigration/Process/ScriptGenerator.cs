using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;

namespace dbmigration.Process
{
    public class ScriptGenerator
    {
        private ILogger _logger;
        private string _outputFile = "";
        private FieldInfo[] _extraFields;


        public ScriptGenerator(ILogger logger, FieldInfo[] extraFields, string outputFile)
        {
            _logger = logger;
            _outputFile = outputFile;
            _extraFields = extraFields;
            if (extraFields != null)
                foreach (var f in extraFields) f.IsExtra = true;
        }

        public void GenerateScript(ComparerResult comparerResult)
        {
            List<string> statements = new List<string>();
            statements.Add("-- SCRIPT START ******************************************\r\n");
            statements.Add("\r\n");

            var schemas = GenerateNewSchemas(comparerResult);

            if (!string.IsNullOrEmpty(schemas))
            {
                statements.Add("-- NEW SCHEMAS ********************************************\r\n");
                statements.Add(schemas);
                statements.Add("\r\n");
            }

            if (comparerResult.NewTables.Count > 0)
            {
                statements.Add("-- NEW TABLES ********************************************\r\n");
                foreach (var res in comparerResult.NewTables)
                {
                    statements.Add(GenerateCreates(res, comparerResult.targetDB));
                }
                statements.Add("\r\n");
                _logger.Log("Generated Scripts New Tables: " + comparerResult.NewTables.Count(), EventType.Info);
            }

            if (comparerResult.ModifiedTables.Count > 0)
            {
                statements.Add("-- UPD TABLES ********************************************\r\n");
                foreach (var res in comparerResult.ModifiedTables)
                {
                    statements.Add(GenerateUpdates(res, comparerResult.targetDB));
                }
                statements.Add("\r\n");
                _logger.Log("Generated Scripts Update Tables: " + comparerResult.ModifiedTables.Count(), EventType.Info);
            }

            List<ComparerTableResult> allTables = new List<ComparerTableResult>();
            allTables.AddRange(comparerResult.UntouchedTables);
            allTables.AddRange(comparerResult.NewTables);
            allTables.AddRange(comparerResult.ModifiedTables);
            allTables = allTables.OrderBy(e => e.FullName).ToList();
            statements.Add("-- ****************************************************\r\n");
            statements.Add("-- ****************     INSERTS      ******************\r\n");
            statements.Add("-- ****************************************************\r\n");

            foreach (var res in allTables)
            {
                statements.Add(GenerateInserts(res, comparerResult.sourceDB, comparerResult.targetDB));
            }
            _logger.Log("Generated Scripts Insert Statements. ", EventType.Info);

            Report(string.Join("", statements));
        }


        private string GenerateNewSchemas(ComparerResult comparerResult)
        {
            string cmdCREATE = "";
            foreach (var res in comparerResult.NewSchemas)
            {
                cmdCREATE += "CREATE SCHEMA [" + comparerResult.targetDB + "].[" + res + "]\r\n";
            }
            return cmdCREATE;
        }


        private string GenerateCreates(ComparerTableResult ctable, string targetDB)
        {
            string cmdCREATE = "CREATE TABLE [" + targetDB + "]." + ctable.FullName + " (\r\n";
            string cmdPrimaryKey = "";
            int fCount = 0;
            int pkCount = 0;

            ctable.NewFields.AddRange(_extraFields);

            foreach (var field in ctable.NewFields)
            {
                if (fCount > 0) cmdCREATE += ",\r\n";
                cmdCREATE += "   [" + field.Name + "] " + field.Type + (field.Nullable ? " NULL" : " NOT NULL");
                if (field.IsPK)
                {
                    if (string.IsNullOrEmpty(cmdPrimaryKey)) cmdPrimaryKey += "   PRIMARY KEY CLUSTERED (\r\n";
                    if (pkCount > 0) cmdPrimaryKey += ",\r\n";
                    cmdPrimaryKey += "\t[" + field.Name + "] ASC";
                    pkCount++;
                }
                fCount++;
            }
            if (!string.IsNullOrEmpty(cmdPrimaryKey))
            {
                cmdPrimaryKey += "\r\n   )";
                cmdCREATE += ",\r\n" + cmdPrimaryKey + "\r\n)\r\n";
            } else
            {
                cmdCREATE += "\r\n)\r\n";
            }
            
            return cmdCREATE;
        }

        private string GenerateUpdates(ComparerTableResult ctable, string targetDB)
        {
            string cmdUPD = "";

            foreach (var field in ctable.NewFields)
            {
                cmdUPD += "ALTER TABLE [" + targetDB + "]." + ctable.FullName + " ADD [" + field.Name + "] " + field.Type + (field.Nullable ? " NULL" : " NOT NULL") + "\r\n";
            }
            foreach (var field in ctable.UpgradedFields)
            {
                cmdUPD += "ALTER TABLE [" + targetDB + "]." + ctable.FullName + " ALTER COLUMN [" + field.Name + "] " + field.Type + (field.Nullable ? " NULL" : " NOT NULL") + "\r\n";
            }

            return cmdUPD;
        }

        private string GenerateInserts(ComparerTableResult ctable, string sourceDB, string targetDB)
        {
            List<FieldInfo> allFields = new List<FieldInfo>();
            allFields.AddRange(_extraFields);
            allFields.AddRange(ctable.UntouchedFields);
            allFields.AddRange(ctable.NewFields);
            allFields.AddRange(ctable.UpgradedFields);
            string cmdUPD = "";
            foreach (var field in allFields)
            {
                cmdUPD += $"DECLARE @{field.Name} as {field.Type}; \r\n";
                if (field.Type.Contains("char") || field.Type.Contains("text") || field.Type.Contains("date") || field.Type.Contains("time"))
                    cmdUPD += $"SET @{field.Name} = '{field.Value}'; \r\n";
                else
                    cmdUPD += $"SET @{field.Name} = {field.Value}; \r\n";
            }

            cmdUPD += "INSERT INTO [" + targetDB + "]." + ctable.FullName + " ( ";

            int fCount = 0;
            foreach (var field in allFields)
            {
                if (fCount > 0) cmdUPD += ", ";
                cmdUPD += $"[{field.Name}]";
                fCount++;
            }

            cmdUPD += ") SELECT ";
            fCount = 0;
            foreach (var field in allFields)
            {
                if (fCount > 0) cmdUPD += ", ";
                if (field.IsExtra) cmdUPD += $"@{field.Name} as [{field.Name}]";
                    else cmdUPD += "[" + field.Name + "]";
                fCount++;
            }

            cmdUPD += " FROM [" + sourceDB + "]." + ctable.FullName + " WITH (NOLOCK) \r\n";

            return cmdUPD;
        }

        private void Report(string message, EventType logType = EventType.None)
        {
           
            if (!string.IsNullOrEmpty(_outputFile))
                File.AppendAllText(_outputFile, message + "\r\n");
        }
    }
}
