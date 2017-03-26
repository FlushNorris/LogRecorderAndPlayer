using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class NamedPipeService : INamedPipeService
    {
        public delegate NamedPipeServerResponse SyncSession(NamedPipeSession namedPipeSession);
        public delegate NamedPipeServerResponse ClosingSession(NamedPipeSession namedPipeSession);

        public event SyncSession OnSyncSession = null;
        public event ClosingSession OnClosingSession = null;

        public string ProcessData(string value)
        {
            //Hvordan skal jeg vide hvad value skal deserlizeres til, da Process data skal bruges for følgende:
            //Browser -> Player: For at fortælle hvilket processId den pågældende browser har fået
            //HttpModule -> Player: For at hente information om hvilket data der skal anvendes 
            //LRAPCommand/LRAPDataReader -> Player: For igen at hente hvilket data der skal anvendes i Requestet og/eller Responset

            var serverRequest = SerializationHelper.Deserialize<NamedPipeServerRequest>(value, SerializationType.Json);
            NamedPipeServerResponse serverResponse = new NamedPipeServerResponse() {Success = true};
            switch (serverRequest.Type)
            {
                case NamedPipeServerRequestType.SyncBrowser:
                {
                    var browser = (NamedPipeSession) serverRequest.Data;
                    if (OnSyncSession != null)
                        serverResponse = OnSyncSession(browser);
                    break;
                }
                case NamedPipeServerRequestType.ClosingBrowser:
                {
                    var browser = (NamedPipeSession) serverRequest.Data;
                    if (OnClosingSession != null)
                        serverResponse = OnClosingSession(browser);
                    break;
                }
                default:
                {
                    serverResponse.Success = false;
                    serverResponse.Message = "Type Not Implemented";
                    break;
                }
            }

            //var callback = OperationContext.Current.GetCallbackChannel<INamedPipeCallbackService>();
            //callback.NotifyClient();

            return SerializationHelper.Serialize(serverResponse, SerializationType.Json);
        }
    }
}
