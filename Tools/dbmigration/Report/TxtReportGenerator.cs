using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;

namespace dbmigration.Process
{
    public class TxtReportGenerator : IReportGenerator
    {
        private ILogger _logger;
        private string _outputFile = "";
        private bool _validOutput = false;

        public TxtReportGenerator(ILogger logger, string outputFile)
        {
            _logger = logger;
            _outputFile = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile) + ".txt");
        }


        public void Report(string message, EventType logType = EventType.None)
        {
            _logger.Log(message, logType);
            if (_validOutput)
                File.AppendAllText(_outputFile, message + "\r\n");
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

                File.AppendAllText(_outputFile, "-- Report file: " + DateTime.Now.ToString() + "\r\n");
                _validOutput = true;
            }
            catch (Exception ex)
            {
                _logger.Log("Error: " + ex.ToString(), EventType.Error);
            }
        }

        public void End()
        {
            File.AppendAllText(_outputFile, "-- End of Report: " + DateTime.Now.ToString() + "\r\n");
        }

    }
}
