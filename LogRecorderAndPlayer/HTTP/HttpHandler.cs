using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace LogRecorderAndPlayer
{
    public class LRAPHttpHandler : IHttpHandler, IRequiresSessionState
    {
        internal readonly IHttpHandler OriginalHandler;

        public LRAPHttpHandler()
        {
            
        }

        public LRAPHttpHandler(IHttpHandler originalHandler)
        {
            OriginalHandler = originalHandler;
        }

        public void ProcessRequest(HttpContext context)
        {
            // do not worry, ProcessRequest() will not be called, but let's be safe
            if (context.Request.CurrentExecutionFilePathExtension.ToLower() == ".lrap")
            {
                return;
            }
            throw new InvalidOperationException("LRAPHttpHandler cannot process requests.");
        }

        public bool IsReusable
        {            
            get { return false; } // IsReusable must be set to false since class has a member!
        }
    }
}
