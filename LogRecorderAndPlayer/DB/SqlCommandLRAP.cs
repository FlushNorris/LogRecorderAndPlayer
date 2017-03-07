using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class SqlCommandLRAP : IDisposable
    {
        public SqlCommand Cmd { get; private set; }

        public SqlCommandLRAP(string sql, SqlConnection conn)
        {
            Cmd = new SqlCommand(sql, conn);
        }

        public SqlParameterCollection Parameters
        {
            get { return Cmd.Parameters; }
        }

        public SqlParameter CreateParameter()
        {
            return Cmd.CreateParameter();
        }

        public SqlDataReaderLRAP ExecuteReader()
        {
            return new SqlDataReaderLRAP(Cmd.ExecuteReader());
        }

        public SqlDataReaderLRAP ExecuteReader(CommandBehavior behavior)
        {
            return new SqlDataReaderLRAP(Cmd.ExecuteReader(behavior));
        }

        public int ExecuteNonQuery()
        {
            return Cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return Cmd.ExecuteScalar();
        }

        public void Dispose()
        {
            Cmd.Dispose();
        }
    }
}
