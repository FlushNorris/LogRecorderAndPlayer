using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class SqlCommandLRAPExtensions
    {
        public static void CreateAndAddInputParameter(this SqlCommandLRAP command, SqlDbType type, string name, object value)
        {
            command.CreateAndAddInputParameter(type, name, value, -1);
        }

        public static void CreateAndAddInputParameter(this SqlCommandLRAP command, SqlDbType type, string name, object value, int size)
        {
            command.CreateAndAddParameter(type, name, value, size, ParameterDirection.Input);
        }

        public static void CreateAndAddInputParameter(this SqlCommandLRAP command, string name, object value, int size = 0)
        {
            SqlParameter p = command.Parameters.AddWithValue(parameterName: name, value: value);
            if (size > 0)
            {
                p.Size = size;
            }
        }

        public static void CreateAndAddParameter(this SqlCommandLRAP command, SqlDbType type, string name, object value, int size, ParameterDirection direction)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = direction;
            parameter.SqlDbType = type;
            parameter.ParameterName = name;
            if (size > 0)
            {
                parameter.Size = size;
            }

            if (value == null)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = value;
            }
            command.Parameters.Add(parameter);
        }

        public static byte ReadByte(this SqlDataReaderLRAP reader, string columnName, byte defaultValueIfNull = 0)
        {
            return ReadValue(reader, columnName, defaultValueIfNull);
        }

        public static short ReadShort(this SqlDataReaderLRAP reader, string columnName, short defaultValueIfNull = -1)
        {
            return ReadValue<short>(reader, columnName, defaultValueIfNull);
        }

        public static int ReadInt(this SqlDataReaderLRAP reader, string columnName, int defaultValueIfNull = -1)
        {
            return ReadValue<int>(reader, columnName, defaultValueIfNull);
        }

        public static byte[] ReadImage(this SqlDataReaderLRAP reader, string columnName, byte[] defaultValueIfNull = null)
        {
            return ReadValue<byte[]>(reader, columnName, defaultValueIfNull);
        }

        public static long ReadLong(this SqlDataReaderLRAP reader, string columnName, long defaultValueIfNull = -1)
        {
            return ReadValue<long>(reader, columnName, defaultValueIfNull);
        }

        public static decimal ReadDecimal(this SqlDataReaderLRAP reader, string columnName, decimal defaultValueIfNull = -1)
        {
            return ReadValue<decimal>(reader, columnName, defaultValueIfNull);
        }

        public static bool ReadBoolean(this SqlDataReaderLRAP reader, string columnName, bool defaultValueIfNull = false)
        {
            return ReadValue<bool>(reader, columnName, defaultValueIfNull);
        }

        public static DateTime ReadDateTime(this SqlDataReaderLRAP reader, string columnName)
        {
            return ReadValue<DateTime>(reader, columnName, DateTime.MinValue);
        }

        public static DateTime ReadDateTime(this SqlDataReaderLRAP reader, string columnName, DateTime defaultValueIfNull)
        {
            return ReadValue<DateTime>(reader, columnName, defaultValueIfNull);
        }

        public static DateTime? ReadDateTimeNullable(this SqlDataReaderLRAP reader, string columnName, DateTime? defaultValueIfNull)
        {
            return ReadValue<DateTime?>(reader, columnName, defaultValueIfNull);
        }

        public static int? ReadIntNullable(this SqlDataReaderLRAP reader, string columnName, int? defaultValueIfNull = null)
        {
            return ReadValue<int?>(reader, columnName, defaultValueIfNull);
        }

        public static string ReadXml(this SqlDataReaderLRAP reader, string columnName, string defaultValueIfNull = "")
        {
            return ReadValue<string>(reader, columnName, defaultValueIfNull);
        }

        public static T ReadValue<T>(this SqlDataReaderLRAP reader, string columnName)
        {
            return ReadValue<T>(reader, columnName, default(T));
        }

        public static T ReadValue<T>(this SqlDataReaderLRAP reader, string columnName, T defaultValueIfNull)
        {
            object value = reader[columnName];

            if (!(value is DBNull))
            {
                return (T)value;
            }
            else
            {
                return defaultValueIfNull;
            }
        }

        public static T ReadValue<T>(this SqlDataReaderLRAP reader, int ordinal)
        {
            return ReadValue<T>(reader, ordinal, default(T));
        }

        public static T ReadValue<T>(this SqlDataReaderLRAP reader, int ordinal, T defaultValueIfNull)
        {
            object value = reader[ordinal];

            if (!(value is DBNull))
            {
                return (T)value;
            }
            else
            {
                return defaultValueIfNull;
            }
        }        
    }
}
