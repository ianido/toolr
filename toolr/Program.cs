using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using toolr.common;
using toolr.common.Options;
using toolr.Options;

namespace toolr
{
    class Program
    {
        public static void Main(string[] args)
        {
            ILogger logger = new Logger();
            GeneralOptions options = OptionsFactory.GetToolOptions(args, logger);

            if (!string.IsNullOrEmpty(options.ParsingError))
            {
                Console.Write("tbstools: ");
                Console.WriteLine(options.ParsingError);
                Console.WriteLine("Try 'tbs --help' for more information.");
                return;
            }

            if (options.ShowHelp)
            {
                ShowHelp(options.HelpText);
            } else
            if (options.Process == null)
            {
                Console.Write("tbs: ");
                Console.WriteLine("No tool selected.");
                Console.WriteLine("Try 'tbs --help' for more information.");
            } else 
                options.Process.Start();

            Console.ResetColor();
            Console.WriteLine("");
            Console.WriteLine("Press enter to close.");
            Console.ReadLine();
        }

        static void ShowHelp(string[] help)
        {
            foreach (string s in help)
                Console.WriteLine(s);
        }
    }
}
