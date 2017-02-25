using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using File = System.IO.File;
using Path = System.IO.Path;

namespace LogRecorderAndPlayer.Logging
{
    public static class LoggingJSON
    {
        public static void LogElements(string filePath, LogHandlerDTO[] logElements)
        {
            foreach (var logElement in logElements)
            {
//                var fileName = $"{logElement.Timestamp.ToString("yyyyMMddHHmmssfff")}_{logElement.GUID}_{(logElement.BundleGUID?.ToString() ?? "NoBundle")}_{logElement.PageGUID}_{logElement.SessionGUID}_{logElement.LogType}_{prepareElementForIO(logElement.Element)}.json";
                var fileName = $"{logElement.Timestamp.ToString("yyyyMMddHHmmssfff")}_{logElement.PageGUID}_{logElement.LogType}_{prepareElementForIO(logElement.Element)}.json";
                var f = File.CreateText(filePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + fileName);
                try
                {
                    f.Write(new JavaScriptSerializer().Serialize(logElement));
                }
                finally
                {
                    f.Close();
                }               
            }
        }

        private static string prepareElementForIO(string element)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();
            foreach (var ch in element)
            {
                if (!invalid.Contains(ch))
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
