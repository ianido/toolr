using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toolr.common
{
    public class Logger : ILogger
    {
        public void Log(string message, EventType type = EventType.None )
        {
            Console.ResetColor();
            switch (type)
            {
                case EventType.Error: { Console.ForegroundColor = ConsoleColor.DarkRed; } break;
                case EventType.Warning: { Console.ForegroundColor = ConsoleColor.DarkYellow; } break;
                case EventType.Info: { Console.ForegroundColor = ConsoleColor.Cyan; } break;                
            }
            if (string.IsNullOrEmpty(message) || message == @"\*" || message == @"*/") { Console.WriteLine(""); return; };
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message);
            Console.ResetColor();
        }
    }
}
