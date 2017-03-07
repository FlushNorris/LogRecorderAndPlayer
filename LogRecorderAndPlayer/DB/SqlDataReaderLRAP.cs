using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class SqlDataReaderLRAP
    {
        public SqlDataReader Reader { get; private set; }

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

        public SqlDataReaderLRAP(SqlDataReader reader)
        {
            Reader = reader;
        }

        public bool Read()
        {
            return Reader.Read();
        }

        public void Close()
        {
            Reader.Close();
        }

        public void Dispose()
        {
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
