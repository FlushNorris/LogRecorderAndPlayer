using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using File = System.IO.File;
using Path = System.IO.Path;

namespace LogRecorderAndPlayer
{
    public class LoggingToJSON : ILoggingPersister
    {
        private readonly static int maxFilePathLength = 248;

        private string BuildFilePath(LogElementDTO logElement, string filePath)
        {
            var timestamp = TimeHelper.UnixTimeStampToDateTime(logElement.UnixTimestamp);
            var fileName = $"{timestamp.ToString("yyyyMMddHHmmssffffff")}_{logElement.SessionGUID}__{logElement.PageGUID}_{logElement.LogType}_{prepareElementForIO(logElement.Element)}";

            var filePathAndName = filePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + fileName;
            var fileExtension = ".json";

            var finalPath = filePathAndName + fileExtension;
            if (finalPath.Length > maxFilePathLength)
            {
                filePathAndName = filePathAndName.Substring(0, maxFilePathLength - fileExtension.Length);
                finalPath = filePathAndName + fileExtension;
            }

            return finalPath;
        }

        private LogElementInfo BuildLogElementInfo(string filePath)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            var r = new Regex("^([^_]+)_+([^_]+)_+([^_]+)_+([^_]+).+$");
            var m = r.Match(fileName);
            if (m.Success)
            {
                var timestamp = DateTime.ParseExact(m.Groups[1].Value, "yyyyMMddHHmmssffffff", null);
                var sessionGUID = new Guid(m.Groups[2].Value);
                var pageGUID = new Guid(m.Groups[3].Value);

                LogType logType;
                if (m.Groups[4].Value == "OnAjaxRequestSend")
                    logType = LogType.OnHandlerRequestSend;
                else if (m.Groups[4].Value == "OnAjaxRequestReceived")
                    logType = LogType.OnHandlerRequestReceived;
                else if (m.Groups[4].Value == "OnAjaxResponseSend")
                    logType = LogType.OnHandlerResponseSend;
                else if (m.Groups[4].Value == "OnAjaxResponseReceived")
                    logType = LogType.OnHandlerResponseReceived;
                else
                    if (!LogType.TryParse(m.Groups[4].Value, false, out logType))
                        throw new Exception($"Unknown logtype ({m.Groups[4].Value})");

                return new LogElementInfo()
                {
                    FilePath = filePath,
                    Timestamp = timestamp,
                    SessionGUID = sessionGUID,
                    PageGUID = pageGUID,
                    LogType = logType
                };
            }

            throw new Exception("Invalid logelement ({fileName})");
        }

        public void LogElement(string filePath, LogElementDTO logElement)
        {
            //return;
            var fileName = BuildFilePath(logElement, filePath);

            Console.WriteLine(fileName);
            var f = File.CreateText(fileName);
            try
            {
                f.Write(new JavaScriptSerializer().Serialize(logElement));
            }
            finally
            {
                f.Close();
            }
        }

        public IEnumerable<LogElementDTO> LoadLogElements(string filePath, DateTime? from, DateTime? to)
        {
            var files = System.IO.Directory.GetFiles(filePath, $"*.json");
            foreach (var file in files)
            {
                var logElementInfo = BuildLogElementInfo(file);
                if ((from == null || from.Value <= logElementInfo.Timestamp) && (to == null || to >= logElementInfo.Timestamp))
                    yield return new JavaScriptSerializer().Deserialize<LogElementDTO>(System.IO.File.ReadAllText(file));
            }
        }

        public LogElementDTO LoadLogElement(LogElementInfo logElementInfo)
        {
            var json = File.ReadAllText(logElementInfo.FilePath);
            return new JavaScriptSerializer().Deserialize<LogElementDTO>(json);
        }

        public LogElementsInfo LoadLogElementsInfo(string filePath, DateTime? from, DateTime? to)
        {
            var result = new LogElementsInfo();
            var files = System.IO.Directory.GetFiles(filePath, $"*.json");
            foreach (var file in files)
            {
                var logElementInfo = BuildLogElementInfo(file);
                if ((from == null || from.Value <= logElementInfo.Timestamp) && (to == null || to >= logElementInfo.Timestamp))
                    result.LogElementInfos.Add(logElementInfo);
            }
            result.LogElementInfos = result.LogElementInfos.OrderBy(x => x.Timestamp).ToList();
            return result;
        }

        private string prepareElementForIO(string element)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (var ch in element)
                if (!invalid.Contains(ch))
                    sb.Append(ch);
            return sb.ToString();
        }
    }
}
