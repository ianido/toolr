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
    public class HtmlReportGenerator : IReportGenerator
    {
        private ILogger _logger;
        private string _outputFile = "";
        private bool _validOutput = false;

        public HtmlReportGenerator(ILogger logger, string outputFile)
        {
            _logger = logger;
            _outputFile = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile) + ".html");
        }
 
        public void Report(string message, EventType logType = EventType.None)
        {
            _logger.Log(message, logType);
            if (_validOutput)
            {
                var color = "black";
                if (logType == EventType.Error) color = "red";
                if (logType == EventType.Info) color = "blue";
                if (logType == EventType.Warning) color = "#f43636";
                message = $"<span style='color:{color}'>" + message + "</br>";
                File.AppendAllText(_outputFile, message);
            }
        }

        public void Init()
        {
            try
            {
                if (File.Exists(_outputFile))
                {
                    _logger.Log("Re-Creating file: " + _outputFile, EventType.Info);
                    File.Delete(_outputFile);
                }
                else
                    _logger.Log("Creating file: " + _outputFile, EventType.Info);

                File.AppendAllText(_outputFile, "<html>");
                File.AppendAllText(_outputFile, "<head><title>Migration Report file</title></head>");
                File.AppendAllText(_outputFile, "<h1>Report file: " + DateTime.Now.ToString() + "</h1>");
                _validOutput = true;
            }
            catch (Exception ex)
            {
                _logger.Log("Error: " + ex.ToString(), EventType.Error);
            }
        }

        public void End()
        {
            File.AppendAllText(_outputFile, "</html>");
        }

    }
}
