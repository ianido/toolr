
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;
using SampleTool.Options;

namespace SampleTool.Process
{
    public class SampleToolEngine : IToolProcess
    {
        private SampleToolOptions _options;
        private ILogger _logger;
        public SampleToolEngine(SampleToolOptions migrationOptions, ILogger logger)
        {
            _options = migrationOptions;
            _logger = logger;
        }

        public void Start()
        {
            _logger.Log("Starting the tool Process");
            _logger.Log("Ending the tool Process");
        }
    }
}
