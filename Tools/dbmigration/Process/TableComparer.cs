using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;

namespace dbmigration.Process
{
    public class TableComparer
    {
        private ILogger _logger;
        private FieldCompatibility _fieldCompatibility;


        public TableComparer(ILogger logger)
        {
            _logger = logger;
            _fieldCompatibility = new FieldCompatibility();
        }

        public ComparerResult Compare(List<TableInfo> source, List<TableInfo> target, string[] targetSchemas)
        {
            _logger.Log("Checking structure differences.", EventType.Info);
            ComparerResult result = new ComparerResult();
            if (source.Count == 0)
            {
                _logger.Log("No tables found in source database.", EventType.Error);
                return result;
            }
            result.sourceDB = source[0].Database;
            if (target.Count == 0)
            {
                _logger.Log("No tables found in target database.", EventType.Error);
                return result;
            }
            result.targetDB = target[0].Database;

            foreach (var table in source)
            {
                bool existTable = target.Exists(t => t.FullName == table.FullName);
                if (!existTable)
                {
                    result.NewTables.Add(CompareTable(table, null));
                    if (targetSchemas.FirstOrDefault(s => s == table.Schema) == null)
                        result.NewSchemas.Add(table.Schema);
                }
                else
                {
                    var targetTable = target.FirstOrDefault(t => t.FullName == table.FullName);
                    ComparerTableResult compTable = CompareTable(table, targetTable);
                    if ((compTable.NewFields.Count > 0) || (compTable.RemovedFields.Count > 0) || (compTable.UpgradedFields.Count > 0))
                        result.ModifiedTables.Add(compTable);
                    else
                        result.UntouchedTables.Add(compTable);
                }                 
            }

            return result;
        }

        public ComparerTableResult CompareTable(TableInfo source, TableInfo target)
        {
            ComparerTableResult result = new ComparerTableResult();
            result.Name = source.Name;
            result.Schema = source.Schema;

            foreach (var field in source.Fields)
            {
                if (target == null) { result.NewFields.Add(field); continue; }
                bool existField = target.Fields.Exists(t => t.Name == field.Name);
                if (!existField)
                {
                    if (ExistField(result.NewFields, field))
                        _logger.Log("Duplicate (NewFields) Field: " + field.Name, EventType.Error);
                    field.Nullable = true;
                    result.NewFields.Add(field);
                }
                else
                {
                    // Compare Field
                    FieldInfo targetFiled = target.Fields.FirstOrDefault(t => t.Name == field.Name);
                    try
                    {
                        if (field == targetFiled)
                        {
                            if (ExistField(result.UntouchedFields, field))
                                _logger.Log("Duplicate (UntouchedFields) Field: " + field.Name, EventType.Error);
                            result.UntouchedFields.Add(field);
                        }
                        else
                        {
                            var uField = _fieldCompatibility.CreateCompatibleField(field, targetFiled);
                            if (ExistField(result.UpgradedFields, uField))
                                _logger.Log("Duplicate (UpgradedFields) Field: " + uField.Name, EventType.Error);
                            result.UpgradedFields.Add(uField);                            
                        }
                    }
                    catch (IncompatibleTypesException ex)
                    {
                        _logger.Log(ex.Message, EventType.Error);
                        result.ErrorFields.Add(field);
                    }
                }
            }
            return result;
        }

        private bool ExistField(List<FieldInfo> list, FieldInfo field)
        {
            return list.Exists(e => e.Name == field.Name);
        }

    }
}
