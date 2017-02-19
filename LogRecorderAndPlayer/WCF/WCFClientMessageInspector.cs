using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogRecorderAndPlayer
{
    public class WCFClientMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            //var via = channel.Via;
            //var inputSession = channel.InputSession;
            //var outputSession = channel.OutputSession;
            //var SessionId = channel.SessionId;
            //var LocalAddress = channel.LocalAddress;
            //var state = channel.State;            

            return null;
            //throw new NotImplementedException();
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            ModifyReceivedRequest(ref reply);
            //ChangeMessage(ref reply);
            //throw new NotImplementedException();
        }

        private void ModifyReceivedRequest(ref Message message)
        {
            var buffer = message.CreateBufferedCopy(int.MaxValue);
            message = buffer.CreateMessage();

            MemoryStream ms = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = encoding };
            writerSettings.ConformanceLevel = ConformanceLevel.Auto;
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms));
            //buffer.WriteMessage(ms);
            message.WriteMessage(writer);
            //message.WriteBodyContents(writer);
            writer.Flush();
            string messageBodyString = encoding.GetString(ms.ToArray());

            // change the message body
            //messageBodyString = messageBodyString.Replace("<HelloWorldResult>Hello World 1337</HelloWorldResult>", "<HelloWorldResult>Hello World HIJACKED!</HelloWorldResult>");

            ms = new MemoryStream(encoding.GetBytes(messageBodyString));
            var dictReader = XmlDictionaryReader.Create(ms);
            //var s = dictReader.ReadString();
            //var s2 = dictReader.ReadContentAsString();

            //XmlReader bodyReader = XmlReader.Create(ms);
            var originalMessage = message;
            //message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, bodyReader);
            message = System.ServiceModel.Channels.Message.CreateMessage(dictReader, int.MaxValue, originalMessage.Version);
            message.Headers.CopyHeadersFrom(originalMessage);            
            //var msgX = message.CreateBufferedCopy(int.MaxValue);
            //message = msgX.CreateMessage();
        }

        private void ModifyReceivedRequestOLD(ref Message message)
        {
            MemoryStream ms = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = encoding };
            writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            writerSettings.Indent = true;
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms, writerSettings));
            //message.WriteBodyContents(writer);            
            //message.WriteStartEnvelope(writer);
            message.WriteMessage(writer);
            writer.Flush();

            ms.Position = 0;            
            var sr = new StreamReader(ms);
            var myStr = sr.ReadToEnd();

            ms.Position = 0;
            XmlDocument xDoc = new XmlDocument();
            //xDoc.
            //xDoc.Load(new StringReader(@"<?xml version=""1.0"" encoding=""UTF-8""?>" + message.ToString()));
            xDoc.Load(ms);
            ms.Flush();
            ms = new MemoryStream();

            // XML stuff

            GC.Collect();
            xDoc.Save(ms);
            ms.Position = 0;
            //XmlWriter xmlWriter = XmlWriter.Create(ms);
            //XmlDictionaryWriter xmlDict = XmlDictionaryWriter.CreateDictionaryWriter(xmlWriter);            
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ConformanceLevel = ConformanceLevel.Fragment;            
            XmlReader xmlReader = XmlReader.Create(ms, readerSettings);
            XmlDictionaryReader xmlDict2 = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            var inner2 = xmlDict2.ReadInnerXml();
            var outer2 = xmlDict2.ReadOuterXml();

            //XmlReader bodyReader = XmlReader.Create(ms);
            Message originalMessage = message;            
            //XmlObjectSerializer xx = new DataContractJsonSerializer(typeof(int));
            //message = Message.CreateMessage(originalMessage.Version, null, message.ToString(), xx);
            message = Message.CreateMessage(xmlDict2, 10000, originalMessage.Version);
            //message = Message.CreateMessage(originalMessage.Version, null, bodyReader);
            message.Headers.CopyHeadersFrom(originalMessage);
            var before = originalMessage.ToString();
            var after = message.ToString();
        }

        private void ChangeMessage(ref System.ServiceModel.Channels.Message messageX)
        {
            MemoryStream ms = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = encoding };
            writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            //XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms));
            var writer = XmlDictionaryWriter.Create(ms);                        
            messageX.WriteBody(writer);
            //messageX.WriteBodyContents(writer);
            writer.Flush();
            string messageBodyString = encoding.GetString(ms.ToArray());

            var xxx = " < Envelope><Header /></Envelope>";
            ms = new MemoryStream();
            writer = XmlDictionaryWriter.Create(ms);
//            writer.Settings.ConformanceLevel = ConformanceLevel.Auto;                
            writer.WriteString(xxx);
//An exception of type 'System.InvalidOperationException' occurred in System.Xml.dll but was not handled in user code
//Additional information: Token Text in state Start would result in an invalid XML document. Make sure that the ConformanceLevel setting is set to ConformanceLevel.Fragment or ConformanceLevel.Auto if you want to write an XML fragment.
            writer.Flush();
            string messageBodyStringWTF = encoding.GetString(ms.ToArray());

            messageBodyString = messageX.ToString();
            //messageBodyString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + message.ToString();

            // change the message body
            //messageBodyString = messageBodyString.Replace("<HelloWorldResult>Hello World 1337</HelloWorldResult>", "<HelloWorldResult>Hello World HIJACKED!</HelloWorldResult>");
            
            //var x = messageX.GetReaderAtBodyContents();
            //messageX.WriteBodyContents(x);

            ms = new MemoryStream(encoding.GetBytes(messageBodyString));
            XmlReader bodyReader = XmlReader.Create(ms);
            System.ServiceModel.Channels.Message originalMessage = messageX;
            //System.ServiceModel.Channels.Message message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, bodyReader);
            System.ServiceModel.Channels.Message message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, writer);
            message.Headers.CopyHeadersFrom(originalMessage);

            messageX = message;


            //ms = new MemoryStream();
            //writerSettings = new XmlWriterSettings { Encoding = encoding };
            //writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms));
            //message.WriteBodyContents(writer);
            //writer.Flush();
            //string messageBodyString3 = encoding.GetString(ms.ToArray());
            //if (messageBodyString != messageBodyString3)
            //{
            //    throw new Exception("NOOOOO");
            //}

            //string messageBodyString4 = message.ToString();

            //if (messageBodyString2 != messageBodyString4)
            //{
            //    throw new Exception("NOOOOO");
            //}
        }

        private void ChangeMessageXXX(ref System.ServiceModel.Channels.Message message)
        {
            MemoryStream ms = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            //XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = encoding };            
            //https://msdn.microsoft.com/en-us/library/system.xml.xmlwritersettings.conformancelevel(v=vs.110).aspx
            //writerSettings.ConformanceLevel = ConformanceLevel.Auto;
            //XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms));
            //message.WriteBodyContents(writer);
            //writer.Flush();
            string messageBodyString = message.ToString();
            //string messageBodyString = encoding.GetString(ms.ToArray());

            var f = System.IO.File.CreateText($"c:\\LogTest\\Response{DateTime.Now.ToString("HHmmssfff")}.txt");
            f.WriteLine(messageBodyString);
            f.Close();

            // change the message body
            //messageBodyString = messageBodyString.Replace("<HelloWorldResult>Hello World 1337</HelloWorldResult>", "<HelloWorldResult>Hello World HIJACKED!</HelloWorldResult>");

            ms = new MemoryStream(encoding.GetBytes(messageBodyString));
            XmlReader bodyReader = XmlReader.Create(ms);
            System.ServiceModel.Channels.Message originalMessage = message;

            var outer = bodyReader.ReadOuterXml();
            var inner = bodyReader.ReadInnerXml();

            message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, bodyReader);
            //message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, new TextBodyWriter(messageBodyString));
                        
            var bDebug = bodyReader.ToString();

            //BodyWriter x = new 

            //message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, "action", messageBodyString);
            //message = System.ServiceModel.Channels.Message.CreateMessage(originalMessage.Version, null, message.GetReaderAtBodyContents());
            //message.Headers.CopyHeadersFrom(originalMessage);

            string messageBodyString2 = message.ToString();

            if (messageBodyString != messageBodyString2)
            {
                throw new Exception("NOOOOO");
            }
        }
    }

    // <summary>
    /// Necessary to write out the contents as text (used with the Raw return type)
    /// </summary>
    public class TextBodyWriter : BodyWriter
    {
        byte[] messageBytes;
        string message;

        public TextBodyWriter(string message)
            : base(true)
        {
            this.message = message;
            this.messageBytes = Encoding.UTF8.GetBytes(message);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteString(message);
            //writer.WriteStartElement("Binary");
            //writer.WriteBase64(this.messageBytes, 0, this.messageBytes.Length);
            //writer.WriteEndElement();
        }
    }

    public class xxx : BodyWriter
    {
        public xxx(bool isBuffered) : base(isBuffered)
        {
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            
        }
    }
}
