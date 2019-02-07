
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;
using dbmigration.Options;

namespace dbmigration.Process
{
    public class Migration : IToolProcess
    {
        private MigrationOptions _options;
        private ILogger _logger;
        public Migration(MigrationOptions migrationOptions, ILogger logger)
        {
            _options = migrationOptions;
            _logger = logger;

            if (migrationOptions.IgnoreFile != null)
            {
                if (!migrationOptions.IgnoreFile.Contains(":"))
                {
                    string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    migrationOptions.IgnoreFile = Path.Combine(basePath, migrationOptions.IgnoreFile);
                }
                try
                {
                    migrationOptions.IgnoreTableList = File.ReadAllLines(migrationOptions.IgnoreFile);
                    logger.Log("Reading ignore file: " + migrationOptions.IgnoreFile + ". " + migrationOptions.IgnoreTableList.Length + " lines found.", EventType.Info);
                }
                catch (Exception ex) { logger.Log("Error reading ignore file: " + migrationOptions.IgnoreFile + ". " + ex.Message, EventType.Error); }
            }

            if (migrationOptions.OutputFile != null)
            {
                if (!migrationOptions.OutputFile.Contains(":"))
                {
                    string basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    migrationOptions.OutputFile = Path.Combine(basePath, migrationOptions.OutputFile);
                }                
            }

        }

        private void GenerateReport(ComparerResult comparerResult, IReportGenerator generator)
        {
            generator.Init();

            generator.Report("/*");
            generator.Report("Untouched Tables: " + comparerResult.UntouchedTables.Count, EventType.Info);
            generator.Report("");

            foreach (var res in comparerResult.UntouchedTables)
            {
                generator.Report(res.FullName, EventType.None);
            }

            generator.Report("");
            generator.Report("New Tables: " + comparerResult.NewTables.Count, EventType.Info);
            generator.Report("");
            foreach (var res in comparerResult.NewTables)
            {
                generator.Report(res.FullName, EventType.None);
            }

            generator.Report("");
            generator.Report("Modified Tables: " + comparerResult.ModifiedTables.Count, EventType.Info);
            generator.Report("");

            foreach (var res in comparerResult.ModifiedTables)
            {
                generator.Report(res.FullName, EventType.None);

                if (res.NewFields.Count > 0)
                {
                    foreach (var f in res.NewFields)
                    {
                        generator.Report("  (NEW) " + f.Name + " " + f.Type + (f.Nullable ? " NULL" : " NOT NULL") + (f.IsPK ? " PK" : ""), EventType.Warning);
                    }
                }

                if (res.UpgradedFields.Count > 0)
                {
                    foreach (var f in res.UpgradedFields)
                    {
                        generator.Report("  (UPG) " + f.Name + " " + f.Type + (f.Nullable ? " NULL" : " NOT NULL") + (f.IsPK ? " PK" : ""), EventType.Warning);
                    }
                }
            }
            generator.Report("*/");
            generator.End();
        }

        public void Start()
        {
            TableIterator tiSource = new TableIterator(_options.Source, _options.IgnoreTableList, _logger);
            TableIterator tiTarget = new TableIterator(_options.Target, _options.IgnoreTableList, _logger);

            var targetSchemas = tiTarget.GetSchemas().ToArray();
            var sourceTable = tiSource.GetTables();
            var targetTable = tiTarget.GetTables();
            

            TableComparer comparer = new TableComparer(_logger);
            var comparisonResult = comparer.Compare(sourceTable, targetTable, targetSchemas);

            if (!string.IsNullOrEmpty(_options.Report))
            {
                IReportGenerator reportGen = null;
                if (_options.Report.ToLower().Trim() == "html")
                    reportGen = new HtmlReportGenerator(_logger, _options.OutputFile);
                else // default report format
                    reportGen = new TxtReportGenerator(_logger, _options.OutputFile);
                GenerateReport(comparisonResult, reportGen);                
            }

            if (_options.GenerateScripts)
            {
                
                var scriptfileName = Path.Combine(Path.GetDirectoryName(_options.OutputFile), Path.GetFileNameWithoutExtension(_options.OutputFile) + ".sql");
                ScriptGenerator scriptGen = new ScriptGenerator(_logger, new FieldInfo[0], _options.OutputFile);
                scriptGen.GenerateScript(comparisonResult);
            }

            if (_options.Execute)
            {
            }
        }
    }
}
