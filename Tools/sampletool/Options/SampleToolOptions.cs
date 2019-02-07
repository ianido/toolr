using NDesk.Options;
using SampleTool.Process;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;
using toolr.common.Options;

namespace SampleTool.Options
{
    public class SampleToolOptions : GeneralOptions
    {
        public string ConnStr { get; set; }
        public string PayGroup { get; set; }
        public string AutopayRegion { get; set; }
        public bool TsIntegration { get; set; }

        public SampleToolOptions()
        { }

        protected override string[] Help(OptionSet p)
        {
            List<string> result = new List<string>();
            result.Add("");
            result.Add("Usage: tbs tool=sampletool [OPTIONS]");
            result.Add("");
            result.Add("This is a sampleTool for demo purposes.");
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
            result.Add("tbs tool=sampletool -r ");
            return result.ToArray();
        }

        public override OptionSet LoadOptions(IEnumerable<string> args, ILogger logger)
        {
            try
            {
                var p = new OptionSet() {
                    { "c|option1=", "the option 1.", v => ConnStr = v },
                    { "p|option2=", "the option 2.", v => PayGroup = v }
                };

                var extra = p.Parse(args);

                HelpText = Help(p);
                Process = new SampleToolEngine(this, logger);

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
