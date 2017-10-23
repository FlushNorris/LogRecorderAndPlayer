using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

//git co 9138b2c176944f7f8cfb30f72c36ac1b90b3fcc1 //nyeste prod version hos top... release4.2.1

namespace LogRecorderAndPlayer
{
    public static class LoggingClientBase
    {
        public static void SetupClientBase<T>(System.ServiceModel.ClientBase<T> client, HttpContext httpContext) where T : class
        {            
            var guid = LoggingHelper.GetInstanceGUID(httpContext, () => new Guid()).GetValueOrDefault();
            var sessionGUID = LoggingHelper.GetSessionGUID(httpContext, httpContext?.Handler as Page, () => new Guid()).GetValueOrDefault();
            var pageGUID = LoggingHelper.GetPageGUID(httpContext, httpContext?.Handler as Page, () => new Guid()).GetValueOrDefault();
            var bundleGUID = LoggingHelper.GetBundleGUID(httpContext, () => new Guid()).GetValueOrDefault();

            var newLogElement = new LogElementDTO(
                guid: guid,
                sessionGUID: sessionGUID,
                pageGUID: pageGUID,
                bundleGUID: bundleGUID, 
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnSetup,
                element: $"SetupClientBase_{client.Endpoint.Address.Uri}", //LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: "", //SerializationHelper.Serialize(requestParams, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null,
                instanceTime: DateTime.Now,
                stackTrace: null
            );

            var eab = new EndpointAddressBuilder(client.Endpoint.Address);
            
            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.GUIDTag,
                                                                string.Empty,
                                                                guid.ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.SessionGUIDTag,
                                                                string.Empty,
                                                                sessionGUID.ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.PageGUIDTag,  
                                                                string.Empty,
                                                                pageGUID.ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.BundleGUIDTag,
                                                    string.Empty,
                                                    bundleGUID.ToString()));

            eab.Headers.Add(AddressHeader.CreateAddressHeader(Consts.ServerGUIDTag,
                                                    string.Empty,
                                                    LoggingHelper.GetServerGUID(httpContext, () => new Guid()).GetValueOrDefault().ToString()));

            client.Endpoint.Address = eab.ToEndpointAddress();
        }
    }
}
