using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public class SqlCommandDTO
    {
        public Guid BundleGUID { get; set; }
        public LoggingDBType Type { get; set; }
        public string CommandText { get; set; }
        public CommandType CommandType { get; set; }
        public CommandBehavior CommandBehavior { get; set; }
        public List<SqlParamDTO> Params { get; set; }

        public SqlCommandDTO()
        {
            Params = new List<SqlParamDTO>();
        }
    }

    public class SqlParamDTO
    {
        public string Name { get; set; }
        public SqlDbType Type { get; set; }
        public string Value { get; set; }
    }

    public enum LoggingDBType
    {
        CmdReader = 0,
        CmdNonQuery = 1,
        CmdScalar = 2
    }

    public class ReaderRowDTO
    {
        public List<string> Values { get; set; }

        public ReaderRowDTO()
        {
            Values = new List<string>();
        }
    }

    public class ReaderResultDTO
    {
        public int ResultIndex { get; set; }
        public List<string> Fields { get; set; }
        public List<ReaderRowDTO> Rows { get; set; }

        public ReaderResultDTO()
        {
            Fields = new List<string>();
            Rows = new List<ReaderRowDTO>();
        }
    }

    public static class LoggingDB
    {
        public static LogElementResponse LogRequest(SqlCommandLRAP cmd, LoggingDBType type, CommandBehavior behavior = CommandBehavior.Default)
        {
            var cmdDTO = MapSqlCommandLRAPToSqlCommandDTO(cmd, type, behavior);

            var result = LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(HttpContext.Current, HttpContext.Current.Handler as Page, () => new Guid()).Value,
                pageGUID: LoggingHelper.GetPageGUID(HttpContext.Current, HttpContext.Current.Handler as Page, () => new Guid()).Value,
                bundleGUID: cmdDTO.BundleGUID,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnPersistenceRequest,
                element: cmdDTO.CommandText,
                element2: null,
                value: SerializationHelper.Serialize(cmdDTO, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null
            ));

            result.Object = cmdDTO;

            return result;
        }

        public static void LogResponse(SqlCommandDTO cmdDTO, object value)
        {
            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(HttpContext.Current, HttpContext.Current.Handler as Page, () => new Guid()).Value,
                pageGUID: LoggingHelper.GetPageGUID(HttpContext.Current, HttpContext.Current.Handler as Page, () => new Guid()).Value,
                bundleGUID: cmdDTO.BundleGUID,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnPersistenceResponse,
                element: cmdDTO.CommandText,
                element2: null,
                value: value != null ? SerializationHelper.Serialize(value, SerializationType.Json) : null,
                times: 1,
                unixTimestampEnd: null
            ));            
        }

        public static ReaderResultDTO MapReaderToReaderResultDTO(SqlDataReader reader, int resultIndex = 0)
        {
            var result = new ReaderResultDTO();
            result.ResultIndex = resultIndex;
            
            var c = reader.FieldCount;
            for (int i = 0; i < c; i++)
                result.Fields.Add(reader.GetName(i));

            return result;
        }

        public static ReaderRowDTO MapReaderToReaderRow(SqlDataReader reader)
        {
            var result = new ReaderRowDTO();

            var c = reader.FieldCount;
            for (int i = 0; i < c; i++)
            {
                var v = reader[i];
                result.Values.Add(v == DBNull.Value ? null : MapObjectToString(v));
            }

            return result;
        }

        private static SqlCommandDTO MapSqlCommandLRAPToSqlCommandDTO(SqlCommandLRAP cmd, LoggingDBType type, CommandBehavior behavior)
        {
            var result = new SqlCommandDTO();
            result.BundleGUID = Guid.NewGuid();
            result.Type = type;
            result.CommandType = cmd.CommandType;
            result.CommandText = cmd.CommandText;
            result.CommandBehavior = behavior;
            foreach (SqlParameter p in cmd.Parameters)
            {
                var param = new SqlParamDTO();
                param.Type = p.SqlDbType;
                param.Value = MapObjectToString(p.Value == DBNull.Value ? null : p.Value);
                result.Params.Add(param);
            }
            return result;
        }

        private static object UnboxNullable(object value)
        {
            var typeOriginal = value.GetType();
            if (typeOriginal.IsGenericType
                && typeOriginal.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // generic value, unboxing needed
                return typeOriginal.InvokeMember("GetValueOrDefault",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, value, null);
            }
            return value;
        }

        private const string DATETIME_FORMAT_ROUNDTRIP = "o"; //ISO 8601 e.g. 2008-06-15T21:15:07.0000000 (https://msdn.microsoft.com/en-us/library/zdtaw1bw(v=vs.110).aspx)
        private static string MapObjectToString(object value)
        {
            try
            {
                if (value == null)
                    return null;

                value = UnboxNullable(value);

                if (value is string
                    || value is char
                    || value is char[]
                    || value is System.Xml.Linq.XElement
                    || value is System.Xml.Linq.XDocument)
                    return $"N'{(value.ToString().Replace("'", "''"))}'";

                if (value is bool)                    
                    return Convert.ToInt32(value).ToString(); // True -> 1, False -> 0

                if (value is sbyte || value is byte || value is short || value is ushort
                    || value is int || value is uint || value is long || value is ulong
                    || value is float || value is double || value is decimal)
                    return value.ToString();                

                // SQL Server only supports ISO8601 with 3 digit precision on datetime,
                // datetime2 (>= SQL Server 2008) parses the .net format, and will 
                // implicitly cast down to datetime.
                // Alternatively, use the format string "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"
                // to match SQL server parsing
                if (value is DateTime)
                    return $"CAST('{((DateTime)value).ToString(DATETIME_FORMAT_ROUNDTRIP)}' as datetime2)";

                if (value is DateTimeOffset)
                    return $"'{((DateTimeOffset) value).ToString(DATETIME_FORMAT_ROUNDTRIP)}'";

                if (value is Guid)
                    return $"'{(Guid)value}'";

                if (value is byte[])
                {
                    var data = (byte[])value;
                    if (data.Length == 0)
                        return null;

                    var sb = new StringBuilder();
                    sb.Append("0x");
                    foreach(var b in data)
                        sb.Append(b.ToString("h2"));
                    return sb.ToString();
                }

                return $"/* UNKNOWN DATATYPE: {value.GetType().ToString()} */ N'{value}'";
            }
            catch (Exception ex)
            {
                return $"/* Exception occurred while converting data: {ex.Message} :: {ex.StackTrace} */";
            }
        }
    }
}
