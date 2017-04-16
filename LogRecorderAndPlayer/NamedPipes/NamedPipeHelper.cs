using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class NamedPipeHelper
    {
        public static bool SendClosingSession(Guid serverGUID, Guid processGUID, int processId)
        {
            var session = new NamedPipeSession() { ProcessGUID = processGUID, ProcessId = processId };
            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.ClosingSession, Data = session };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            if (!String.IsNullOrWhiteSpace(error))
                throw new Exception(error);

            var serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);
            return !serverResponse.Success;
        }

        public static LogElementDTO FetchLogElementFromPlayer(Guid serverGUID, Guid pageGUID, LogType logType)
        {
            var fetchLogElement = new NamedPipeFetchLogElement() { PageGUID = pageGUID, LogType = logType };
            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.FetchLogElement, Data = fetchLogElement };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            string serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            if (!String.IsNullOrWhiteSpace(error))
                throw new Exception(error);
            var serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);
            return (LogElementDTO)serverResponse.Data;
        }

        public static void SetHandlerLogElementAsDone(Guid serverGUID, Guid pageGUID, LogType logType, string handlerUrl, JobStatus jobStatus) //, bool async)
        {
            var async = false;

            var data = new NamedPipeBrowserJob() { PageGUID = pageGUID, LogType = logType, HandlerUrl = handlerUrl, JobStatus = jobStatus };

            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.BrowserJobComplete, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error, async);
            if (async && serverResponseJSON == null)
                return;
            NamedPipeServerResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }

        public static void SetLogElementAsDone(Guid serverGUID, Guid pageGUID, Guid? logElementGUID, JobStatus jobStatus) //, bool async)
        {
            var async = false;

            var data = new NamedPipeBrowserJob() { PageGUID = pageGUID, LogElementGUID = logElementGUID, JobStatus = jobStatus };

            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.BrowserJobComplete, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error, async);
            if (async && serverResponseJSON == null)
                return;
            NamedPipeServerResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }

        public static void SendBrowserJob_ASYNC(NamedPipeSession session, LogElementDTO logElement)
        {
            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.BrowserJob, Data = logElement };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;            
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(session.ProcessGUID, serverRequestJSON, out error, async:true);
            if (serverResponseJSON == null)
                return;
            NamedPipeServerResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while sending starting browser job ({error ?? serverResponse.Message})");
        }

        public static void SendSyncSession(Guid serverGUID, Guid sessionGUID, int processId) //Process.GetCurrentProcess().Id
        {
            var data = new NamedPipeSession() { ProcessGUID = sessionGUID, ProcessId = processId };

            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.SyncSession, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            NamedPipeServerResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }
    }
}
