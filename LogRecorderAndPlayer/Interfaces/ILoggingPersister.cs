using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public interface ILoggingPersister
    {
        void LogElement(string filePath, LogElementDTO logElement);
        IEnumerable<LogElementDTO> LoadLogElements(string filePath, DateTime? from, DateTime? to);
        LogElementDTO LoadLogElement(LogElementInfo logElementInfo);
        LogElementsInfo LoadLogElementsInfo(string filePath, DateTime? from, DateTime? to);
    }
}
