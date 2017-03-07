using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static partial class SqlDataReaderExtensions
    {
        public static string GetInt32SafeToString(this SqlDataReaderLRAP me, int no, string defaultIfNull = "")
        {
            return me.IsDBNull(no) ? defaultIfNull : me.GetInt32(no).ToString();
        }

        public static int GetInt32SafeToInt(this SqlDataReaderLRAP me, int no, int defaultIfNull = -1)
        {
            return me.IsDBNull(no) ? defaultIfNull : me.GetInt32(no);
        }

        public static Decimal GetDecimal(this SqlDataReaderLRAP me, int no, Decimal defaultIfNull = -1)
        {
            return me.IsDBNull(no) ? defaultIfNull : me.GetDecimal(no);
        }

        public static int? GetInt32Nullable(this SqlDataReaderLRAP me, int no)
        {
            return me.IsDBNull(no) ? (int?)null : (int?)me.GetInt32(no);
        }

        public static DateTime? GetDateTimeNullable(this SqlDataReaderLRAP me, int no)
        {
            return me.IsDBNull(no) ? (DateTime?)null : (DateTime?)me.GetDateTime(no);
        }

        public static T GetEnum<T>(this SqlDataReaderLRAP me, object defaultValue, int no) where T : struct, IConvertible
        {
            int value = me.GetInt32(no);

            if (!Enum.IsDefined(typeof(T), value))
                return (T)defaultValue;

            return (T)(object)value;
        }

        public static T GetEnum<T>(this SqlDataReaderLRAP me, object defaultValue, string columnName) where T : struct, IConvertible
        {
            int value = Convert.ToInt32(me[columnName]);

            if (!Enum.IsDefined(typeof(T), value))
                return (T)defaultValue;

            return (T)(object)value;
        }

        public static string GetDateTimeSafeToString(this SqlDataReaderLRAP me, int no, string defaultIfNull = "")
        {
            return me.IsDBNull(no) ? defaultIfNull : me.GetDateTime(no).ToLongDateString() + " : " + me.GetDateTime(no).ToLongTimeString();
        }

        public static string GetDateTimeSafeToString(this SqlDataReaderLRAP me, int no)
        {
            return me.IsDBNull(no) ? DateTime.MinValue.ToString() : me.GetDateTime(no).ToString();
        }

        public static DateTime GetDateTimeSafeToDateTime(this SqlDataReaderLRAP me, int no)
        {
            return me.IsDBNull(no) ? DateTime.MinValue : me.GetDateTime(no);
        }

        public static T GetValue<T>(this SqlDataReaderLRAP me, string columnName)
        {
            return me[columnName] == DBNull.Value ? default(T) : (T)me[columnName];
        }

        public static T GetValue<T>(this SqlDataReaderLRAP me, int columnNumber)
        {
            return me.IsDBNull(columnNumber) ? default(T) : (T)me[columnNumber];
        }

        public static T GetValue<T>(this SqlDataReaderLRAP me, string columnName, T defaultIfNull)
        {
            if (!me.HasRows)
                return defaultIfNull;

            return me[columnName] == DBNull.Value ? defaultIfNull : (T)me[columnName];
        }

        public static T GetValue<T>(this SqlDataReaderLRAP me, int columnNumber, T defaultIfNull)
        {
            return me.IsDBNull(columnNumber) ? defaultIfNull : (T)me[columnNumber];
        }
    }
}
