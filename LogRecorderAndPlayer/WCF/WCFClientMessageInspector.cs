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
using System.Web;
using System.Web.UI;
using System.Xml;

namespace LogRecorderAndPlayer
{
    public class WCFClientMessageInspector : IClientMessageInspector
    {
        private class LoggingState
        {
            public Guid GUID { get; set; }
            public Guid SessionGUID { get; set; }
            public Guid PageGUID { get; set; }
            public Guid BundleGUID { get; set; }
            public string Url { get; set; }
            public string Action { get; set; }
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            //var via = channel.Via;
            //var inputSession = channel.InputSession;
            //var outputSession = channel.OutputSession;
            //var SessionId = channel.SessionId;
            //var LocalAddress = channel.LocalAddress;
            //var state = channel.State;            

            var loggingState = new LoggingState()
            {
                GUID = LoggingHelper.GetInstanceGUID(HttpContext.Current, () => Guid.NewGuid()).GetValueOrDefault(),
                SessionGUID = LoggingHelper.GetSessionGUID(HttpContext.Current, HttpContext.Current?.Handler as Page, () => Guid.NewGuid()).GetValueOrDefault(),
                PageGUID = LoggingHelper.GetPageGUID(HttpContext.Current, HttpContext.Current?.Handler as Page, () => Guid.NewGuid()).GetValueOrDefault(),
                BundleGUID = LoggingHelper.GetBundleGUID(HttpContext.Current, () => Guid.NewGuid()).GetValueOrDefault(),
                Url = LoggingHelper.StripUrlForLRAP(channel.Via.ToString()),
                Action = request.Headers.Action
            };            

            string messageBody = GetMessageBody(request);

            LoggingHelper.LogElement(new LogElementDTO(
                guid: loggingState.GUID,
                sessionGUID: loggingState.SessionGUID,
                pageGUID: loggingState.PageGUID,
                bundleGUID: loggingState.BundleGUID,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnWCFServiceRequest,
                element: Path.GetFileName(loggingState.Action),
                element2: null,
                value: messageBody,
                times: 1,
                unixTimestampEnd: null
            ));

            request = BuildMessage(messageBody, request);

            return loggingState;
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            var loggingState = correlationState as LoggingState;
            if (loggingState != null)
            {
                string messageBody = GetMessageBody(reply);

                LoggingHelper.LogElement(new LogElementDTO(
                    guid: loggingState.GUID,
                    sessionGUID: loggingState.SessionGUID,
                    pageGUID: loggingState.PageGUID,
                    bundleGUID: loggingState.BundleGUID,
                    progressGUID: null,
                    unixTimestamp: TimeHelper.UnixTimestamp(),
                    logType: LogType.OnWCFServiceResponse,
                    element: Path.GetFileName(loggingState.Action),
                    element2: null,
                    value: messageBody,
                    times: 1,
                    unixTimestampEnd: null
                ));

                reply = BuildMessage(messageBody, reply);
            }
        }

        private string GetMessageBody(Message message) //BuildMessage must be called afterwards to build a new message, because we cannot read from the message more than once
        {
            var buffer = message.CreateBufferedCopy(int.MaxValue);
            message = buffer.CreateMessage();

            MemoryStream ms = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            XmlWriterSettings writerSettings = new XmlWriterSettings { Encoding = encoding };
            writerSettings.ConformanceLevel = ConformanceLevel.Auto;
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(ms));
            message.WriteMessage(writer);
            writer.Flush();
            return encoding.GetString(ms.ToArray());
        }

        private Message BuildMessage(string messageBody, Message originalMessage)
        {
            // change the message body
            //messageBodyString = messageBodyString.Replace("<HelloWorldResult>Hello World 1337</HelloWorldResult>", "<HelloWorldResult>Hello World HIJACKED!</HelloWorldResult>");

            Encoding encoding = Encoding.UTF8;
            MemoryStream ms = new MemoryStream(encoding.GetBytes(messageBody));
            var dictReader = XmlDictionaryReader.Create(ms);

            var message = System.ServiceModel.Channels.Message.CreateMessage(dictReader, int.MaxValue, originalMessage.Version);
            //message.Headers.CopyHeadersFrom(originalMessage);

            return message;
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
