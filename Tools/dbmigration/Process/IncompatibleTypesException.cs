using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbmigration.Process
{
    public class IncompatibleTypesException : Exception
    {
        public IncompatibleTypesException(string message) : base(message)
        {
        }
    }
}
