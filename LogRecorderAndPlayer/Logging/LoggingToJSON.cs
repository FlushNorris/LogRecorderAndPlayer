using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using File = System.IO.File;
using Path = System.IO.Path;

namespace LogRecorderAndPlayer
{
    public static class LoggingToJSON
    {
        private readonly static int maxFilePathLength = 248;

        public static void LogElement(string filePath, LogHandlerDTO logElement)
        {
            var fileName = $"{logElement.Timestamp.ToString("yyyyMMddHHmmssfff")}_{logElement.PageGUID}_{logElement.LogType}_{prepareElementForIO(logElement.Element)}";            
            var filePathAndName = filePath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + fileName;
            var fileExtension = ".json";

            var finalPath = filePathAndName + fileExtension;
            if (finalPath.Length > maxFilePathLength)
            {
                filePathAndName = filePathAndName.Substring(0, maxFilePathLength - fileExtension.Length);
                finalPath = filePathAndName + fileExtension;
            }

            var f = File.CreateText(finalPath);
            try
            {
                f.Write(new JavaScriptSerializer().Serialize(logElement));
            }
            finally
            {
                f.Close();
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
