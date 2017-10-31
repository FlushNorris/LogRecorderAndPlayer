﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using LogRecorderAndPlayer.Common;

namespace LogRecorderAndPlayer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerCommunicationService : PlayerCommunicationServiceInterface
    {
        //Session/Browser to Server/Player
        public delegate TransferElementResponse SyncSession(TransferElementSession namedPipeSession);
        public delegate TransferElementResponse ClosingSession(TransferElementSession namedPipeSession);
        public delegate TransferElementResponse BrowserJobComplete(TransferElementBrowserJob namedPipeBrowserJob);
        public delegate TransferElementResponse LogElementHistory(LogElementDTO previousLogElement, LogElementDTO nextLogElement, AdditionalData additionalData);
        public delegate TransferElementResponse LogElementDifference(LogElementDTO previousLogElement, LogElementDTO nextLogElement);

        //Server/Player to Session/Browser
        public delegate TransferElementResponse BrowserJob(LogElementDTO logElement);

        //Webservice to Player
        public delegate TransferElementResponse FetchLogElement(TransferElementFetchLogElement fetchLogElement);

        public event SyncSession OnSyncSession = null;
        public event ClosingSession OnClosingSession = null;
        public event BrowserJob OnBrowserJob = null;
        public event BrowserJobComplete OnBrowserJobComplete = null;
        public event FetchLogElement OnFetchLogElement = null;
        public event LogElementHistory OnLogElementHistory = null;
        public event LogElementDifference OnLogElementDifference = null;

        public string ProcessData(string value)
        {
            var serverRequest = JsonHelper.Deserialize<TransferElementRequest>(value);
            var serverResponse = new TransferElementResponse() { Success = true };
            switch (serverRequest.Type)
            {
                case TransferElementRequestType.SyncSession:
                {
                        var session = JsonHelper.Deserialize<TransferElementSession>(serverRequest.Data.ToString());
                        //var session = (TransferElementSession)serverRequest.Data;
                        if (OnSyncSession != null)
                            serverResponse = OnSyncSession(session);
                        break;
                    }
                case TransferElementRequestType.ClosingSession:
                    {
                        var session = JsonHelper.Deserialize<TransferElementSession>(serverRequest.Data.ToString());
                        //var session = (TransferElementSession)serverRequest.Data;
                        if (OnClosingSession != null)
                            serverResponse = OnClosingSession(session);
                        break;
                    }
                case TransferElementRequestType.FetchLogElement:
                    {
                        var fetchLogElement = JsonHelper.Deserialize<TransferElementFetchLogElement>(serverRequest.Data.ToString());
                        //var fetchLogElement = (TransferElementFetchLogElement)serverRequest.Data;
                        if (OnFetchLogElement != null)
                            serverResponse = OnFetchLogElement(fetchLogElement);
                        break;
                    }
                case TransferElementRequestType.BrowserJobComplete:
                    {
                        var browserJob = JsonHelper.Deserialize<TransferElementBrowserJob>(serverRequest.Data.ToString());
                        //var browserJob = (TransferElementBrowserJob)serverRequest.Data;
                        if (OnBrowserJobComplete != null)
                            serverResponse = OnBrowserJobComplete(browserJob);
                        break;
                    }
                case TransferElementRequestType.BrowserJob:
                    {
                        var logElement = JsonHelper.Deserialize<LogElementDTO>(serverRequest.Data.ToString());
                        //var logElement = (LogElementDTO)serverRequest.Data;
                        if (OnBrowserJob != null)
                            serverResponse = OnBrowserJob(logElement);
                        break;
                    }
                case TransferElementRequestType.LogElementHistory:
                    {
                        var logElementHistory = JsonHelper.Deserialize<TransferLogElementHistory>(serverRequest.Data.ToString());
                        //var logElementHistory = (TransferLogElementHistory) serverRequest.Data;
                        if (OnLogElementHistory != null)
                            serverResponse = OnLogElementHistory(logElementHistory.PreviousLogElement, logElementHistory.NextLogElement, logElementHistory.AdditionalData);
                        break;
                    }
                case TransferElementRequestType.ReportDifference:
                    {
                        var logElementHistory = JsonHelper.Deserialize<TransferLogDifference>(serverRequest.Data.ToString());
                        //var logElementHistory = (TransferLogDifference)serverRequest.Data;
                        if (OnLogElementDifference != null)
                            serverResponse = OnLogElementDifference(logElementHistory.PreviousLogElement, logElementHistory.NextLogElement);
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

            return JsonHelper.Serialize(serverResponse);
        }
    }
}
