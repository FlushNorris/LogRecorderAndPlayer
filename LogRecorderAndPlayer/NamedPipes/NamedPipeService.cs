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
        //Session/Browser to Server/Player
        public delegate TransferElementResponse SyncSession(TransferElementSession namedPipeSession); 
        public delegate TransferElementResponse ClosingSession(TransferElementSession namedPipeSession);
        public delegate TransferElementResponse BrowserJobComplete(TransferElementBrowserJob namedPipeBrowserJob);

        //Server/Player to Session/Browser
        public delegate TransferElementResponse BrowserJob(LogElementDTO logElement); 

        //Webservice to Player
        public delegate TransferElementResponse FetchLogElement(TransferElementFetchLogElement fetchLogElement);

        public event SyncSession OnSyncSession = null;
        public event ClosingSession OnClosingSession = null;
        public event BrowserJob OnBrowserJob = null;
        public event BrowserJobComplete OnBrowserJobComplete = null;
        public event FetchLogElement OnFetchLogElement = null;

        public string ProcessData(string value)
        {
            //Hvordan skal jeg vide hvad value skal deserlizeres til, da Process data skal bruges for følgende:
            //Browser -> Player: For at fortælle hvilket processId den pågældende browser har fået
            //HttpModule -> Player: For at hente information om hvilket data der skal anvendes 
            //LRAPCommand/LRAPDataReader -> Player: For igen at hente hvilket data der skal anvendes i Requestet og/eller Responset

            var serverRequest = SerializationHelper.Deserialize<TransferElementRequest>(value, SerializationType.Json);
            var serverResponse = new TransferElementResponse() {Success = true};
            switch (serverRequest.Type)
            {
                case TransferElementRequestType.SyncSession:
                {
                    var session = (TransferElementSession) serverRequest.Data;
                    if (OnSyncSession != null)
                        serverResponse = OnSyncSession(session);
                    break;
                }
                case TransferElementRequestType.ClosingSession:
                {
                    var session = (TransferElementSession) serverRequest.Data;
                    if (OnClosingSession != null)
                        serverResponse = OnClosingSession(session);
                    break;
                }
                case TransferElementRequestType.FetchLogElement:
                {
                    var fetchLogElement = (TransferElementFetchLogElement)serverRequest.Data;
                    if (OnFetchLogElement != null)
                        serverResponse = OnFetchLogElement(fetchLogElement);
                    break;
                }
                case TransferElementRequestType.BrowserJobComplete:
                {
                    var browserJob = (TransferElementBrowserJob)serverRequest.Data;
                    if (OnBrowserJobComplete != null)
                        serverResponse = OnBrowserJobComplete(browserJob);
                    break;
                }
                case TransferElementRequestType.BrowserJob:
                {                        
                    var logElement = (LogElementDTO) serverRequest.Data;
                    if (OnBrowserJob != null)
                        serverResponse = OnBrowserJob(logElement);
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
