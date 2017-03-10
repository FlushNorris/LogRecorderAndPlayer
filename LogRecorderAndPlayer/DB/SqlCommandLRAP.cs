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

        public SqlCommandLRAP()
        {
            Cmd = new SqlCommand();
        }

        public SqlCommandLRAP(string sql, SqlConnection conn)
        {
            Cmd = new SqlCommand(sql, conn);
        }

        public SqlCommandLRAP(string sql, SqlConnection conn, SqlTransaction trans)
        {
            Cmd = new SqlCommand(sql, conn, trans);
        }

        public int CommandTimeout
        {
            get { return Cmd.CommandTimeout; }
            set { Cmd.CommandTimeout = value; }
        }

        public SqlTransaction Transaction
        {
            get { return Cmd.Transaction; }
            set { Cmd.Transaction = value; }
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

        public async Task<SqlDataReaderLRAP> ExecuteReaderAsync(CommandBehavior behavior = CommandBehavior.Default)
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdReader, behavior);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            var result = await Cmd.ExecuteReaderAsync(behavior);
            return new SqlDataReaderLRAP(cmdDto, result);
        }

        public async Task<object> ExecuteScalarAsync()
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdNonQuery);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            var cmdResult = await Cmd.ExecuteScalarAsync();
            LoggingDB.LogResponse(cmdDto, cmdResult);
            return cmdResult;
        }

        public async Task<int> ExecuteNonQueryAsync()
        {
            var reqResult = LoggingDB.LogRequest(this, LoggingDBType.CmdNonQuery);
            var cmdDto = reqResult.Object as SqlCommandDTO;
            var cmdResult = await Cmd.ExecuteNonQueryAsync();
            LoggingDB.LogResponse(cmdDto, cmdResult);
            return cmdResult;
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
            Cmd = null;
            //GC.SuppressFinalize(this);
        }        
    }
}
