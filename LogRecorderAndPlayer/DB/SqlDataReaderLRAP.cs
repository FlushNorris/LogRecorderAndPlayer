using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class SqlDataReaderLRAP : IDisposable
    {
        private SqlCommandDTO CommandDTO { get; set; }
        public SqlDataReader Reader { get; private set; }

        private ReaderResultDTO CurrentReaderResultDTO { get; set; }
        private int CurrentResultIndex { get; set; }

        public object this[string name]
        {
            get
            {
                return Reader[name];
            }
        }

        public object this[int i]
        {
            get
            {
                return Reader[i];
            }
        }

        public SqlDataReaderLRAP(SqlCommandDTO cmdDTO, SqlDataReader reader)
        {
            CommandDTO = cmdDTO;
            Reader = reader;
            CurrentResultIndex = 0;
            CurrentReaderResultDTO = null;
        }

        private void DoLogging()
        {
            LoggingDB.LogResponse(CommandDTO, CurrentReaderResultDTO);
            CurrentReaderResultDTO = null;
        }

        public bool NextResult()
        {
            var result = Reader.NextResult();
            if (result)
            {
                DoLogging();
                CurrentResultIndex++;
                CurrentReaderResultDTO = null;
            }
            return result;
        }

        public bool Read()
        {
            var result = Reader.Read();
            if (CurrentReaderResultDTO == null)
            {
                CurrentReaderResultDTO = LoggingDB.MapReaderToReaderResultDTO(Reader, CurrentResultIndex);
            }
            if (result)
            {
                var readerRow = LoggingDB.MapReaderToReaderRow(Reader);
                CurrentReaderResultDTO.Rows.Add(readerRow);
            }
            return result;
        }

        public void Close()
        {
            DoLogging();
            Reader.Close();
        }

        public void Dispose()
        {
            DoLogging();
            Reader.Dispose();
        }

        public bool IsDBNull(int i)
        {
            return Reader.IsDBNull(i);
        }

        public int GetInt32(int i)
        {
            return Reader.GetInt32(i);
        }

        public DateTime GetDateTime(int i)
        {
            return Reader.GetDateTime(i);
        }

        public bool HasRows => Reader.HasRows;
    }
}
