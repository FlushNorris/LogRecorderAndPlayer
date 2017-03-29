using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class LoggingToCSV : ILoggingPersister
    {
        public void LogElement(string filePath, LogElementDTO logElement)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LogElementDTO> LoadLogElements(string filePath, DateTime? @from, DateTime? to)
        {
            throw new NotImplementedException();
        }

        public LogElementDTO LoadLogElement(LogElementInfo logElementInfo)
        {
            throw new NotImplementedException();
        }

        public LogElementsInfo LoadLogElementsInfo(string filePath, DateTime? @from, DateTime? to)
        {
            throw new NotImplementedException();
        }
    }
}
