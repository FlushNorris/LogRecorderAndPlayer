using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using LogRecorderAndPlayer;

namespace SolutionSpecificAssembly
{
    public class SolutionSpecificClass : ILoggingPlayer
    {
        public bool StoreLogElementHistory(LogElementDTO previousLogElement, LogElementDTO nextLogElement)
        {
            return previousLogElement.LogType == LogType.OnPageRequest;
        }

        public AdditionalData BuildAdditionalData(HttpApplication httpApplication)
        {
            var httpContext = httpApplication.Context;
            var page = httpContext.CurrentHandler as Page;
            if (page != null)
            {
                //...
            }
            var r = new AdditionalData();
            r.Data.Add("SomeKey", "SomeData");
            return r;
        }

        public string FinalizeUrl(List<Tuple<LogElementDTO, LogElementDTO, AdditionalData>> previousServersideLogElements, string url)
        {
            //AbsolutePath    "/PageCallingWebService.aspx"   string
            //AbsoluteUri "http://localhost:61027/PageCallingWebService.aspx?hejhej=1234" string
            //Authority   "localhost:61027"   string
            //DnsSafeHost "localhost" string
            //Fragment    ""  string
            //Host    "localhost" string
            //HostNameType    Dns System.UriHostNameType
            //IdnHost "localhost" string
            //IsAbsoluteUri   true    bool
            //IsDefaultPort   false   bool
            //IsFile  false   bool
            //IsLoopback  true    bool
            //IsUnc   false   bool
            //LocalPath   "/PageCallingWebService.aspx"   string
            //OriginalString  "http://localhost:61027/PageCallingWebService.aspx?hejhej=1234" string
            //PathAndQuery    "/PageCallingWebService.aspx?hejhej=1234"   string
            //Port    61027   int
            //Query   "?hejhej=1234"  string
            //Scheme  "http"  string
            //-Segments    { string[2]}
            //            string[]
            //[0] "/" string
            //[1] "PageCallingWebService.aspx"    string
            //UserEscaped false   bool
            //UserInfo    ""  string


            Uri uri = new Uri(url);

            var pageName = uri.AbsolutePath.Trim('/').ToLower();
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (pageName == "secondpage.aspx")
            {
                var newId = Int32.Parse(query["id"]);
                for (int i = previousServersideLogElements.Count - 1; i >= 0; i++)
                {
                    var loggedElement = previousServersideLogElements[i].Item1;
                    var currentElement = previousServersideLogElements[i].Item2;

                    if (loggedElement.LogType == LogType.OnPageSessionBefore || loggedElement.LogType == LogType.OnPageRequest)
                    {
                        var loggedUri = new Uri("http://" + loggedElement.Element.Trim('/').ToLower());
                        var loggedPageName = loggedUri.AbsolutePath.Trim('/').ToLower();
                        var loggedQuery = HttpUtility.ParseQueryString(loggedUri.Query);
                        if (loggedPageName == "firstpage.aspx")
                        {
                            var loggedId = Int32.Parse(loggedQuery["id"]);
                            if (loggedId == newId)
                            {
                                var currentUri = new Uri("http://" + currentElement.Element.Trim('/').ToLower());
                                var currentQuery = HttpUtility.ParseQueryString(currentUri.Query);
                                query["id"] = currentQuery["id"];

                                var queryArray = (from key in query.AllKeys
                                                  from value in query.GetValues(key)
                                                  select $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}").ToArray();

                                var newUrl = uri.AbsoluteUri.Substring(0, string.IsNullOrEmpty(uri.Query) ? int.MaxValue : uri.AbsoluteUri.IndexOf(uri.Query)) +
                                             (queryArray.Length > 0 ? "?" + string.Join("&", queryArray) : "");

                                uri = new Uri(newUrl);                                
                            }
                        }
                    }
                }
            }

            return uri.ToString();
            //WebHelper.AddQryStrElement()
        }
    }
}
