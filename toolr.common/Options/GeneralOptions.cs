using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace toolr.common.Options
{
    public class GeneralOptions
    {
        public string Tool { get; set; }

        public bool ShowHelp { get; set; }

        public string[] HelpText { get; set; }

        public string ParsingError { get; set; }

        public IToolProcess Process;

        protected virtual string[] Help(OptionSet p)
        {
            List<string> result = new List<string>();
            result.Add("");
            result.Add("Usage: tbs tool=[toolname] [OPTIONS]");
            result.Add("");
            result.Add("Options:");
            result.Add("");
            var mem = new MemoryStream();
            var str = new StreamWriter(mem);
            p.WriteOptionDescriptions(str);
            str.Flush();
            result.Add(System.Text.Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length));
            str.Close();
            return result.ToArray();
        }

        public virtual OptionSet LoadOptions(IEnumerable<string> args, ILogger logger)
        {
            try
            {
                var p = new OptionSet() {
                    { "tool=", "Tool to run: dbmigration, cleanup, etc", v => Tool = v },
                    { "h|help",  "show this message and exit", v => ShowHelp = v != null },
                };
                var extra = p.Parse(args);
                HelpText = Help(p);
                Process = null;
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
