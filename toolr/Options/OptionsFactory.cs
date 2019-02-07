using dbmigration.Options;
using SampleTool.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;
using toolr.common.Options;


namespace toolr.Options
{
    public class OptionsFactory
    {
        public static GeneralOptions GetToolOptions(IEnumerable<string> arguments, ILogger logger)
        {
            var goptions = new GeneralOptions();
            goptions.LoadOptions(arguments, logger);
            
            switch (goptions.Tool)
            {
                case "tbsmigration":
                    {
                        var options = new MigrationOptions();
                        options.LoadOptions(arguments, logger);
                        return options;
                    }
                case "sampletool":
                    {
                        var options = new SampleToolOptions();
                        options.LoadOptions(arguments, logger);
                        return options;
                    }
            }
            return goptions;
        }
    }
}
