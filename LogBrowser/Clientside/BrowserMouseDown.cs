using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogBrowser
{
    public class BrowserMouseDown //Løbe alle jQuery's events igennem som matcher MouseDown og køre dem... og så også lige de extra events der måtte være bundet op til onmousedown
    {
        public Dictionary<string, string> attributes { get; set; }
        public string[] events { get; set; }
        public BrowserMouseDownValue value { get; set; }
        public string element { get; set; }
    }

    public class BrowserMouseDownValue
    {
        public int button { get; set; }
        public bool shiftKey { get; set; }
        public bool altKey { get; set; }
        public bool ctrlKey { get; set; }
    }
}
