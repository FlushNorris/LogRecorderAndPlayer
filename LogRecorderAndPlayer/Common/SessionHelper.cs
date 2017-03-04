using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace LogRecorderAndPlayer
{
    public static class SessionHelper
    {
        public static void Clear(HttpSessionState session) //Must be called where Session.Clear and Session.Abandon are called
        {
            var sessionGUID = LoggingHelper.GetSessionGUID(session);
            session.Clear();
            if (sessionGUID != null)
                LoggingHelper.SetSessionGUID(session, sessionGUID.Value);
        }

        public static void RemoveAll(HttpSessionState session) //Must be called where Session.Clear and Session.Abandon are called
        {
            var sessionGUID = LoggingHelper.GetSessionGUID(session);
            session.Clear();
            if (sessionGUID != null)
                LoggingHelper.SetSessionGUID(session, sessionGUID.Value);
        }

        public static void Abandon(HttpSessionState session) //Must be called where Session.Clear and Session.Abandon are called
        {
            var sessionGUID = LoggingHelper.GetSessionGUID(session);
            session.Abandon();
            if (sessionGUID != null)
                LoggingHelper.SetSessionGUID(session, sessionGUID.Value);
        }
    }
}
