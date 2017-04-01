using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class NamedPipeHelper
    {
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

        public static void SetLogElementAsDone(Guid serverGUID, Guid pageGUID, Guid logElementGUID)
        {
            var data = new NamedPipeBrowserJob() {PageGUID = pageGUID, LogElementGUID = logElementGUID};

            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.BrowserJobComplete, Data = data };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(serverGUID, serverRequestJSON, out error);
            NamedPipeServerResponse serverResponse = null;
            if (error == null)
                serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

            if (error != null || !serverResponse.Success)
                throw new Exception($"Error occured while syncing with player ({error ?? serverResponse.Message})");
        }
    }
}
