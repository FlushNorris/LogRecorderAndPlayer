using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogBrowser
{
    [DataContract]
    public class BrowserMouseDown //Løbe alle jQuery's events igennem som matcher MouseDown og køre dem... og så også lige de extra events der måtte være bundet op til onmousedown
    {
        [DataMember]
        public Dictionary<string, string> attributes { get; set; }
        [DataMember]
        public string[] events { get; set; }
        [DataMember]
        public BrowserMouseDownValue value { get; set; }
    }

    [DataContract]
    public class BrowserMouseDownValue
    {
        [DataMember]
        public int button { get; set; }
        [DataMember]
        public bool shiftKey { get; set; }
        [DataMember]
        public bool altKey { get; set; }
        [DataMember]
        public bool ctrlKey { get; set; }
    }
}
