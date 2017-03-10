using System;
using System.Collections.Generic;
using System.Data;
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
            get { return Reader[name]; }
        }

        public object this[int i]
        {
            get { return Reader[i]; }
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
            Reader = null;
            //GC.SuppressFinalize(this);
        }

        public bool IsDBNull(int i)
        {
            return Reader.IsDBNull(i);
        }

        public object GetValue(int i)
        {
            return Reader.GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            return Reader.GetFieldType(i);
        }

        public int GetInt32(int i)
        {
            return Reader.GetInt32(i);
        }

        public decimal GetDecimal(int i)
        {
            return Reader.GetDecimal(i);
        }

        public Guid GetGuid(int i)
        {
            return Reader.GetGuid(i);
        }

        public Char GetChar(int i)
        {
            return Reader.GetChar(i);
        }

        public bool GetBoolean(int i)
        {
            return Reader.GetBoolean(i);
        }

        public double GetDouble(int i)
        {
            return Reader.GetDouble(i);
        }

        public byte GetByte(int i)
        {
            return Reader.GetByte(i);
        }

        public short GetInt16(int i)
        {
            return Reader.GetInt16(i);
        }

        public long GetInt64(int i)
        {
            return Reader.GetInt64(i);
        }

        public float GetFloat(int i)
        {
            return Reader.GetFloat(i);
        }

        public string GetString(int i)
        {
            return Reader.GetString(i);
        }

        public DateTime GetDateTime(int i)
        {             
            return Reader.GetDateTime(i);
        }

        public DataTable GetSchemaTable()
        {
            return Reader.GetSchemaTable();
        }

        public string GetName(int i)
        {
            return Reader.GetName(i);
        }

        public int FieldCount => Reader.FieldCount;

        public bool HasRows => Reader.HasRows;
    }
}
