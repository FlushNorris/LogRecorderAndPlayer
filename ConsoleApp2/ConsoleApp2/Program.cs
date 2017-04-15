using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using LogBrowser;
using LogRecorderAndPlayer;
using mshtml;

namespace ConsoleApp2
{
    class Program
    {
        public enum Andeby
        {
            Rip=0,
            Rap=1,
            Rup=2
        }

        private static IEnumerable<string> BuildEnu(int x)
        {
            for (int i = 0; i < x; i++)
            {
                Console.WriteLine($"return {i} ({(DateTime.Now)})");
                yield return i.ToString();
            }
        }

        static void Main(string[] args)
        {
            //var lst1 = BuildEnu(5);

            //foreach (var s in lst1)
            //{
            //    Console.WriteLine($"Read1 {s}");
            //}
            //System.Threading.Thread.Sleep(2000);
            //foreach (var s in lst1)
            //{
            //    Console.WriteLine($"Read2 {s}");
            //}

            ///////////////////////////////

            //var p = new Program();
            //var typeName = p.GetType().ToString();
            //Console.WriteLine(typeName);

            //var t2 = Type.GetType(typeName);
            //Console.WriteLine(t2.ToString());

            ///////////////////////////////

            //var nvc1 = new NameValueCollection();
            //nvc1["rip"] = "111";
            //nvc1["rap"] = "222";
            //nvc1["rup"] = "333";
            //nvc1["rup"] = "444";

            //foreach (var key in nvc1.AllKeys)
            //{
            //    Console.WriteLine($"{key} = {nvc1[key]}");
            //}

            /////////////////////


            //            string x = "qwe";
            //            Console.WriteLine($"x = {(x is String)}");
            //            x = null;
            //            Console.WriteLine($"x = {(x is String)}");


            //            string response = @"

            //<!DOCTYPE html>

            //<html xmlns=""http://www.w3.org/1999/xhtml"">
            //<head><title>

            //</title>
            //</head>
            //<script src=""/scripts/jquery-3.1.1.min.js""></script>
            //<link rel=""stylesheet"" href=""/scripts/jquery-ui.min.css"">
            //<script src=""/scripts/jquery-ui.min.js""></script>
            //<body>
            //    <form method=""post"" action=""./FirstPage.aspx?lrap-serverguid=4b46d6ec-d983-4f07-97d0-161cd9143850&amp;lrap-pageguid=16421c80-3a48-4271-ad34-ea19e417ad4f&amp;lrap-playing=1"" id=""form1"">
            //<div class=""aspNetHidden"">
            //<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""Xm4k495avciz52tfuFfQ/kiW/nM1T8ypo2bJ7oKHxeCxGwNLxFoldxJ2WbHvN6XadtZUbevZ76qCcKTcpWukWaq5RIxoitqjmb3ym/VJa+c="" />
            //</div>

            //<div class=""aspNetHidden"">

            //	<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""156D3223"" />
            //	<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""+Uwlak9is5QD37VL26d2nhaWP6+Yl9knLTLaUnH6LMl9lSULfrKzYtr7L5hNA5bvYz5HCGfolw1xvEhDxTJulOCAyW38oEVQCtr1Zm4O0LGrgWqkVuhYsaxinf/kXFFYJKLvfEWqDVIiL++cPhYphw=="" />
            //</div>
            //    <div>

            //    Server textbox: <input name=""ctl00$ContentPlaceHolder1$serverTextbox"" type=""text"" id=""ContentPlaceHolder1_serverTextbox"" /><br/>
            //    <input type=""submit"" name=""ctl00$ContentPlaceHolder1$serverButton"" value=""Fetch current time"" id=""ContentPlaceHolder1_serverButton"" /><br/>
            //    <br/>
            //    <br/>
            //    Client textbox with no id: <input class=""clientTextboxWithNoID""/><br/>
            //    Client textbox with id: <input id=""clientTextboxWithID""/><br/>
            //    <br/>
            //    <br/>
            //    <a href=""http://www.google.dk"">Google default</a><br/>
            //    <a href=""http://www.google.dk"" target=""_blank"">Google _blank</a><br/>
            //    <a href=""http://www.google.dk"" target=""_self"">Google _self</a><br/>
            //    <a href=""http://www.google.dk"" target=""_parent"">Google _parent</a><br/>
            //    <a href=""http://www.google.dk"" target=""_top"">Google _top</a><br/>


            //    </div>
            //    </form>
            //</body>
            //</html>
            //";

            //            var html = new HTMLDocument();
            //            var doc = (IHTMLDocument2)html;
            //            doc.write(new object[] { response });

            //            var nvc = new NameValueCollection();

            //            var enu = doc.all.GetEnumerator();
            //            while (enu.MoveNext())
            //            {
            //                var elm = enu.Current;

            //                Console.WriteLine(elm.GetType());
            //            }

            //            string result = html.documentElement.outerHTML;

            //////////////////////////////

            //string sx = $"{{}}";
            //Console.WriteLine(sx);

            //var hmm = new BrowserMouseDown();
            //hmm.attributes = new Dictionary<string, string>();
            //hmm.attributes["what"] = "butt";
            //hmm.attributes["what2"] = "butt2";

            //var jsonResult = SerializationHelper.Serialize(hmm, SerializationType.Json);

            //var json1 = "{\"attributes\":[{\"name\":\"ctl00$ContentPlaceHolder1$serverTextbox\"},{\"id\":\"ContentPlaceHolder1_serverTextbox\"},{\"type\":\"text\"}],\"events\":[],\"value\":{\"button\":0,\"shiftKey\":false,\"altKey\":true,\"ctrlKey\":false}}";
            //var json2 = "{\"events\":[],\"value\":{\"button\":5,\"shiftKey\":false,\"altKey\":true,\"ctrlKey\":false}}";

            //var obj = SerializationHelper.Deserialize<BrowserMouseDown>(json1, SerializationType.Json);


            var s = @""" />AWBCDAW BCDAW CBDATOD<!DOCTYPE html>

<html xmlns=""http://www.w3.org/1999/xhtml"">
<head><title>

</title>
</head>
<script src=""/scripts/jquery-3.1.1.min.js""></script>
<link rel=""stylesheet"" href=""/scripts/jquery-ui.min.css"">
<script src=""/scripts/jquery-ui.min.js""></script><script type=""text/javascript"" src=""/logrecorderandplayerjs.lrap?v=636274548300000000""></script><script type=""text/javascript"">logRecorderAndPlayer.init(""669ff16d-3e12-40d2-aaf0-ce38e2a2d9a3"", ""49618e82-649e-4eb5-9cf2-dd848d38b625"", """");</script>
<body>
    <form method=""post"" action=""./FirstPage.aspx"" id=""form1"">
<div class=""aspNetHidden"">
<input type=""hidden"" name=""wgat"" id=""what"" value=""/eRcjbZ24MjLi/cKzmsv0oKlnNZ6EB1pVeMswZ9H/hfj/XQNq+hzxUV/>JOvFP65mndga+QyFlpFaiWVHoYuoZHabl1D7XHmETqX952RsUb80SA="" />
<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""/eRcjbZ24MabjLi/cKzmsv0oKlnNZ6EB1pVeMswZ9H/hfj/XQNq+hzxUVJOvFP65mndga+QyFlpFaiWVHoYuoZHl1D7XHmETqX952RsUb80SA="" />
</div>

<div class=""aspNetHidden"">

	<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""156D3223"" />
	<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""ufOUSnm/3nJaW+YSwrkjC9w8XMcncV3Hjx+1FnF0bjNXrbEAyOKuZEvJIyiYPCFvuZi5vmQDDAkrZHhPOJHqMDxx4EIXg+Z6VlD31Z78ejGYODtCjiP1dL5P4KyjY9NwuL2VAasb/kz/5O2Oef/htA=="" />
</div>
    <div>
        
    Server textbox: <input name=""ctl00$ContentPlaceHolder1$serverTextbox"" type=""text"" id=""ContentPlaceHolder1_serverTextbox"" /><br/>
    <input type=""submit"" name=""ctl00$ContentPlaceHolder1$serverButton"" value=""Fetch current time"" id=""ContentPlaceHolder1_serverButton"" /><br/>
    <br/>
    <br/>";

            //var r = new Regex("<input.+\"__VIEWSTATE\".*value=\"(.*)\".*/>");
            //var r = new Regex("<input.+\"(__VIEWSTATEGENERATOR)\".*value=\"(.*)\".*/>");
            ////var r = new Regex("<input.+\"__EVENTVALIDATION\".*value=\"(.*)\".*/>");            

            ////var r = new Regex("(<input(?! />)+\"__VIEWSTATE\")");
            ////var r = new Regex("((?!AB)..)"); //Vil lede efter det første eksemplar af en section der ikke starter med "ab".. det burde jo være "bc"
            ////var r = new Regex("((?!\"\\s*/\\>)..)"); //Vil lede efter det første eksemplar af en section der ikke starter med "\"/>" eller "\" />"  eller "\"               />"
            ////var r = new Regex("<input((?!\"\\s*/\\>).*)\"__VIEWSTATE\"");
            ////var r = new Regex("(<input(?!\"\\s*/\\>).*)\"__VIEWSTATE\"");

            ////var r = new Regex("(A((?!WBC).)*D)"); //Må vel næsten betyde at WBC ikke må komme i den rækkefølge?

            ////var xxx = r.Replace(s, "QWERTY");

            //var m = r.Match(s);
            //if (m.Success)
            //{
            //    //^(?!.*ab).*$
            //    Console.WriteLine(m.Groups[1].Value);
            //    var m1 = m.Groups[2];
            //    //m1.Index
            //    //m1.Length

            //    var xxx = s.Substring(0, m1.Index) + "LOLOL" + s.Substring(m1.Index + m1.Length);
            //    Console.WriteLine(xxx);

            //    Console.WriteLine("Ja : "+m1);
            //}
            //else
            //{
            //    Console.WriteLine("Nej");
            //}

            /////////////////////////////////////////////////////

            //var nvc = GetResponseViewState(s);
            //nvc.AllKeys.ToList().ForEach(x => nvc[x] = "???");

            //var html2 = SetResponseViewState(s, nvc);

            //Console.Write(html2);

            /////////////////////////////////////////////////////

            //var nvc1 = new NameValueCollection();
            //nvc1["rip"] = "and";
            //nvc1["rap"] = "and";
            //nvc1["rup"] = "and";

            //var nvc2 = new NameValueCollection();
            //nvc2["super"] = "mand";
            //nvc2["bat"] = "mand";
            //nvc2["spider"] = "mand";

            //var nvc3 = new NameValueCollection();
            //nvc3.Add(nvc1);
            //nvc3.Add(nvc2);

            //Console.WriteLine(string.Join(" ", nvc3.AllKeys));

            //////////////////////////////////////////////////

            var u1 = TimeHelper.UnixTimestamp();
            var dt1 = TimeHelper.UnixTimeStampToDateTime(u1).ToString("yyyyMMddHHmmssffffff");
            var u2 = TimeHelper.UnixTimestamp();
            var dt2 = TimeHelper.UnixTimeStampToDateTime(u2).ToString("yyyyMMddHHmmssffffff");

            Console.WriteLine(u1);
            Console.WriteLine(dt1);
            Console.WriteLine(u2);
            Console.WriteLine(dt2);

            Console.ReadKey(true);

        }

        public static class TimeHelper
        {
            public static double UnixTimestamp(DateTime? dt = null) //Unix timestamp is seconds past epoch
            {
                if (dt == null)
                    dt = DateTime.Now;

                return (double)(dt.Value - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000.0;
            }

            public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);                
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp); //.ToLocalTime();                
                return dtDateTime;
            }
        }

        private static void ExecuteViewStateValueMatchesOrdered(string html, Action<Match> f)
        {
            var rViewState = new Regex("<input.+\"(__VIEWSTATE)\".*value=\"(.*)\".*/>");
            var rViewStateGenerator = new Regex("<input.+\"(__VIEWSTATEGENERATOR)\".*value=\"(.*)\".*/>");
            var rEventValidation = new Regex("<input.+\"(__EVENTVALIDATION)\".*value=\"(.*)\".*/>");

            var lst = new List<Match> { rViewState.Match(html), rViewStateGenerator.Match(html), rEventValidation.Match(html) };

            lst.Where(x => x.Success).OrderBy(x => x.Index).ToList().ForEach(f);
        }

        public static NameValueCollection GetResponseViewState(string html)
        {
            var nvc = new NameValueCollection();

            ExecuteViewStateValueMatchesOrdered(html, m =>
            {
                var tagName = m.Groups[1].Value;
                var value = m.Groups[2].Value;
                nvc[tagName] = value;
            });

            return nvc;

            //var doc1 = new HTMLDocument();
            //var doc2 = (IHTMLDocument2)doc1;
            //doc2.write(new object[] { html });


            //var enu = doc2.all.GetEnumerator();
            //while (enu.MoveNext())
            //{
            //    var elm = enu.Current;
            //    if (elm is mshtml.HTMLInputElement)
            //    {
            //        var input = (mshtml.HTMLInputElement)elm;
            //        if (input.name == "__VIEWSTATE" || input.name == "__VIEWSTATEGENERATOR" || input.name == "__EVENTVALIDATION")
            //        {
            //            nvc[input.name] = input.value;
            //        }
            //    }
            //}
        }

        public static string SetResponseViewState(string html, NameValueCollection nvc)
        {
            StringBuilder sb = new StringBuilder();
            int currPosition = 0;
            ExecuteViewStateValueMatchesOrdered(html, m =>
            {
                var tagName = m.Groups[1].Value;
                var valueObj = m.Groups[2];
                var value = nvc[tagName];
                if (value != null)
                {
                    sb.Append(html.Substring(currPosition, valueObj.Index - currPosition));
                    sb.Append(value);
                    currPosition = valueObj.Index + valueObj.Length;
                }
            });
            if (currPosition < html.Length)
                sb.Append(html.Substring(currPosition));

            return sb.ToString();

            //var rViewState = new Regex("<input.+\"(__VIEWSTATE)\".*value=\"(.*)\".*/>");
            //var rViewStateGenerator = new Regex("<input.+\"(__VIEWSTATEGENERATOR)\".*value=\"(.*)\".*/>");
            //var rEventValidation = new Regex("<input.+\"(__EVENTVALIDATION)\".*value=\"(.*)\".*/>");

            //var lst = new List<Match>();
            //lst.Add(rViewState.Match(html));
            //lst.Add(rViewStateGenerator.Match(html));
            //lst.Add(rEventValidation.Match(html));

            //lst = lst.Where(x => x.Success).OrderByDescending(x => x.Index).ToList();

            //var nvc = new NameValueCollection();

            //lst.ForEach(m =>
            //{
            //    var tagName = m.Groups[1].Value;
            //    var value = m.Groups[2].Value;
            //    nvc[tagName] = value;
            //});

            //var doc1 = new HTMLDocument();
            //var doc2 = (IHTMLDocument2)doc1;
            //doc2.write(new object[] { html });

            //var enu = doc2.all.GetEnumerator();
            //while (enu.MoveNext())
            //{
            //    var elm = enu.Current;
            //    if (elm is mshtml.HTMLInputElement)
            //    {
            //        var input = (mshtml.HTMLInputElement)elm;
            //        if (input.name == "__VIEWSTATE" || input.name == "__VIEWSTATEGENERATOR" || input.name == "__EVENTVALIDATION")
            //        {
            //            input.value = nvc[input.name];
            //        }
            //    }
            //}

            //var r = doc1.documentElement.outerHTML; //strips hopefully unnecessary quotes
            //return r;
        }
    }
}
