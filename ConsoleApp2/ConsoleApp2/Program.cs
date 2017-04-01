using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


            string response = @"

<!DOCTYPE html>

<html xmlns=""http://www.w3.org/1999/xhtml"">
<head><title>

</title>
</head>
<script src=""/scripts/jquery-3.1.1.min.js""></script>
<link rel=""stylesheet"" href=""/scripts/jquery-ui.min.css"">
<script src=""/scripts/jquery-ui.min.js""></script>
<body>
    <form method=""post"" action=""./FirstPage.aspx?lrap-serverguid=4b46d6ec-d983-4f07-97d0-161cd9143850&amp;lrap-pageguid=16421c80-3a48-4271-ad34-ea19e417ad4f&amp;lrap-playing=1"" id=""form1"">
<div class=""aspNetHidden"">
<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""Xm4k495avciz52tfuFfQ/kiW/nM1T8ypo2bJ7oKHxeCxGwNLxFoldxJ2WbHvN6XadtZUbevZ76qCcKTcpWukWaq5RIxoitqjmb3ym/VJa+c="" />
</div>

<div class=""aspNetHidden"">

	<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""156D3223"" />
	<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""+Uwlak9is5QD37VL26d2nhaWP6+Yl9knLTLaUnH6LMl9lSULfrKzYtr7L5hNA5bvYz5HCGfolw1xvEhDxTJulOCAyW38oEVQCtr1Zm4O0LGrgWqkVuhYsaxinf/kXFFYJKLvfEWqDVIiL++cPhYphw=="" />
</div>
    <div>
        
    Server textbox: <input name=""ctl00$ContentPlaceHolder1$serverTextbox"" type=""text"" id=""ContentPlaceHolder1_serverTextbox"" /><br/>
    <input type=""submit"" name=""ctl00$ContentPlaceHolder1$serverButton"" value=""Fetch current time"" id=""ContentPlaceHolder1_serverButton"" /><br/>
    <br/>
    <br/>
    Client textbox with no id: <input class=""clientTextboxWithNoID""/><br/>
    Client textbox with id: <input id=""clientTextboxWithID""/><br/>
    <br/>
    <br/>
    <a href=""http://www.google.dk"">Google default</a><br/>
    <a href=""http://www.google.dk"" target=""_blank"">Google _blank</a><br/>
    <a href=""http://www.google.dk"" target=""_self"">Google _self</a><br/>
    <a href=""http://www.google.dk"" target=""_parent"">Google _parent</a><br/>
    <a href=""http://www.google.dk"" target=""_top"">Google _top</a><br/>


    </div>
    </form>
</body>
</html>
";

            var html = new HTMLDocument();
            var doc = (IHTMLDocument2)html;
            doc.write(new object[] { response });

            var nvc = new NameValueCollection();

            var enu = doc.all.GetEnumerator();
            while (enu.MoveNext())
            {
                var elm = enu.Current;
                if (elm is mshtml.HTMLInputElement)
                {
                    var input = (mshtml.HTMLInputElement) elm;
                    if (input.name == "__VIEWSTATE" || input.name == "__VIEWSTATEGENERATOR" || input.name == "__EVENTVALIDATION")
                    {
                        nvc[input.name] = input.value;
                    }                    
                    Console.WriteLine($"{input.GetType()}: {input.name}={input.value}");
                    input.value = "what";
                }                
            }

            var enu2 = doc.all.GetEnumerator();
            while (enu2.MoveNext())
            {
                var elm = enu2.Current;
                if (elm is mshtml.HTMLInputElement)
                {
                    var input = (mshtml.HTMLInputElement)elm;

                    Console.WriteLine($"{input.GetType()}: {input.name}={input.value}");
                    input.value = "what";
                }
            }

            string result = html.documentElement.outerHTML;

            Console.ReadKey(true);

        }
    }
}
