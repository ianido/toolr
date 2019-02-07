using dbmigration.Process;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr;
using toolr.common;
using toolr.common.Options;

namespace dbmigration.Options
{
    public class MigrationOptions : GeneralOptions
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string IgnoreFile { get; set; }
        public string ExtraFields { get; set; }
        public string[] IgnoreTableList { get; set; }
        public string Report { get; set; }
        public string OutputFile { get; set; }
        public bool GenerateScripts { get; set; }
        public bool Execute { get; set; }
        

        public MigrationOptions()
        {
            IgnoreTableList = new string[0];
            Report = null;
            OutputFile = "report-" + DateTime.Now.ToString("yyyy-MM-dd-hhmmss");
            GenerateScripts = false;
            IgnoreFile = "ignorelist.txt";
        }


        protected override string[] Help(OptionSet p)
        {
            List<string> result = new List<string>();
            result.Add("");
            result.Add("Usage: tbs tool=dbmigration [OPTIONS]");
            result.Add("");
            result.Add("Migrate one DB to another DB; ");
            result.Add("");
            result.Add("Options:");
            result.Add("");
            var mem = new MemoryStream();
            var str = new StreamWriter(mem);
            p.WriteOptionDescriptions(str);
            str.Flush();            
            result.Add(System.Text.Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length));
            str.Close();
            result.Add("");
            result.Add("Example:");
            result.Add("");
            result.Add("tbs tool=dbmigration -r -source='<connection String>' -target='<connection String>'");
            return result.ToArray();
        }

        public override OptionSet LoadOptions(IEnumerable<string> args, ILogger logger)
        {
            try
            {

                var p = new OptionSet() {
                    { "s|source=", "the source Connection String.", v => Source = v },
                    { "t|target=", "the target Connection String.", v => Target = v },
                    { "i|ignorelist=", "specify the file containing the ignore table list. (def: " + IgnoreFile + ")", v => IgnoreFile = v },
                    { "f|extrafields=", "specify the extra fields to add.", v => ExtraFields = v },
                    { "o|output=", "Generate output file.", v => OutputFile = v },
                    { "r|report=", "Report format; html, text.", v => Report = v },
                    { "g|generatescripts", "Generate the scripts.", v => GenerateScripts = v != null }
                };

                var extra = p.Parse(args);

                HelpText = Help(p);
                Process = new Migration(this, logger);

                if (string.IsNullOrEmpty(Report) && !GenerateScripts)
                    ShowHelp = true;

                return p;
            }
            catch (OptionException e)
            {
                ParsingError = e.Message;
                return null;
            }
        }
    }
}
