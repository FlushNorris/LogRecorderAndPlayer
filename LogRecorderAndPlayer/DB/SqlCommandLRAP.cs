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

        public CommandType CommandType
        {
            get { return Cmd.CommandType; }
            set { Cmd.CommandType = value; }
        }

        public SqlConnection Connection
        {
            get { return Cmd.Connection; }
            set { Cmd.Connection = value; }
        }

        public string CommandText
        {
            get { return Cmd.CommandText; }
            set { Cmd.CommandText = value; }
        }

        public SqlParameterCollection Parameters
        {
            get { return Cmd.Parameters; }
        }

        public SqlParameter CreateParameter()
        {
            return Cmd.CreateParameter();
        }

        public SqlDataReaderLRAP ExecuteReader(CommandBehavior behavior = CommandBehavior.Default)
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdReader, behavior);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            
            return new SqlDataReaderLRAP(cmdDto, Cmd.ExecuteReader(behavior));
        }

        public int ExecuteNonQuery()
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdNonQuery);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            var cmdResult = Cmd.ExecuteNonQuery();
            LoggingDB.LogResponse(cmdDto, cmdResult);
            return cmdResult;
        }

        public object ExecuteScalar()
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdScalar);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            var cmdResult = Cmd.ExecuteScalar();
            LoggingDB.LogResponse(cmdDto, cmdResult);
            return cmdResult;
        }

        public void Dispose()
        {
            Cmd.Dispose();
        }        
    }
}
