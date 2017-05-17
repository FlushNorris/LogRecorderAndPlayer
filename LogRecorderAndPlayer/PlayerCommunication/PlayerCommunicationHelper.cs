using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class PlayerCommunicationHelper
    {
        public static bool SendClosingSession(Guid serverGUID, Guid processGUID, int processId)
        {
            var session = new TransferElementSession() { ProcessGUID = processGUID, ProcessId = processId };
            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.ClosingSession, Data = session };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            if (!String.IsNullOrWhiteSpace(error))
                throw new Exception(error);

            var serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);
            return !serverResponse.Success;
        }

        public static FetchLogElementResponse FetchLogElementFromPlayer(Guid serverGUID, Guid pageGUID, LogType logType)
        {
            var fetchLogElement = new TransferElementFetchLogElement() { PageGUID = pageGUID, LogType = logType };
            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.FetchLogElement, Data = fetchLogElement };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            string serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            if (!String.IsNullOrWhiteSpace(error))
                throw new Exception(error);
            var serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);
            return (FetchLogElementResponse)serverResponse.Data;
        }

        public static void SetHandlerLogElementAsDone(Guid serverGUID, Guid pageGUID, LogType logType, string handlerUrl, JobStatus jobStatus) //, bool async)
        {
            var async = false;

            var data = new TransferElementBrowserJob() { PageGUID = pageGUID, LogType = logType, HandlerUrl = handlerUrl, JobStatus = jobStatus };

            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.BrowserJobComplete, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error, async);
            if (async && serverResponseJSON == null)
                return;
            TransferElementResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }

        public static void SetLogElementAsDone(Guid serverGUID, Guid pageGUID, Guid? logElementGUID, JobStatus jobStatus) //, bool async)
        {
            var async = false;

            var data = new TransferElementBrowserJob() { PageGUID = pageGUID, LogElementGUID = logElementGUID, JobStatus = jobStatus };

            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.BrowserJobComplete, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error, async);
            if (async && serverResponseJSON == null)
                return;
            TransferElementResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }

        public static void SendBrowserJob_ASYNC(TransferElementSession session, LogElementDTO logElement)
        {
            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.BrowserJob, Data = logElement };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(session.ProcessGUID, serverRequestJSON, out error, async: true);
            if (serverResponseJSON == null)
                return;
            TransferElementResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while sending starting browser job ({error ?? serverResponse.Message})");
        }

        public static void SendSyncSession(Guid serverGUID, Guid sessionGUID, int processId) //Process.GetCurrentProcess().Id
        {
            var data = new TransferElementSession() { ProcessGUID = sessionGUID, ProcessId = processId };

            var serverRequest = new TransferElementRequest() { Type = TransferElementRequestType.SyncSession, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = PlayerCommunicationClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            TransferElementResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<TransferElementResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while communicating with player ({error ?? serverResponse.Message})");
        }
    }
}
