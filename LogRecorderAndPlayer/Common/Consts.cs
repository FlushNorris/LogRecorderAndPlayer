using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class Consts
    {
        public static readonly string GUIDTag = "lrap-guid";
        public static readonly string SessionGUIDTag = "lrap-sessionguid";
        public static readonly string PageGUIDTag = "lrap-pageguid";
        public static readonly string BundleGUIDTag = "lrap-bundleguid";

        public static readonly string[] ForbiddenRequestParams = new string[] {
            "ASP.NET_SessionId",
            "ALL_HTTP",
            "ALL_RAW",
            "APPL_MD_PATH",
            "APPL_PHYSICAL_PATH",
            "AUTH_TYPE",
            "AUTH_USER",
            "AUTH_PASSWORD",
            "LOGON_USER",
            "REMOTE_USER",
            "CERT_COOKIE",
            "CERT_FLAGS",
            "CERT_ISSUER",
            "CERT_KEYSIZE",
            "CERT_SECRETKEYSIZE",
            "CERT_SERIALNUMBER",
            "CERT_SERVER_ISSUER",
            "CERT_SERVER_SUBJECT",
            "CERT_SUBJECT",
            "CONTENT_LENGTH",
            "CONTENT_TYPE",
            "GATEWAY_INTERFACE",
            "HTTPS",
            "HTTPS_KEYSIZE",
            "HTTPS_SECRETKEYSIZE",
            "HTTPS_SERVER_ISSUER",
            "HTTPS_SERVER_SUBJECT",
            "INSTANCE_ID",
            "INSTANCE_META_PATH",
            "LOCAL_ADDR",
            "PATH_INFO",
            "PATH_TRANSLATED",
            "QUERY_STRING",
            "REMOTE_ADDR",
            "REMOTE_HOST",
            "REMOTE_PORT",
            "REQUEST_METHOD",
            "SCRIPT_NAME",
            "SERVER_NAME",
            "SERVER_PORT",
            "SERVER_PORT_SECURE",
            "SERVER_PROTOCOL",
            "SERVER_SOFTWARE",
            "URL",
            "HTTP_CACHE_CONTROL",
            "HTTP_CONNECTION",
            "HTTP_CONTENT_LENGTH",
            "HTTP_CONTENT_TYPE",
            "HTTP_ACCEPT",
            "HTTP_ACCEPT_ENCODING",
            "HTTP_ACCEPT_LANGUAGE",
            "HTTP_COOKIE",
            "HTTP_HOST",
            "HTTP_REFERER",
            "HTTP_USER_AGENT",
            "HTTP_X_REQUESTED_WITH",
            };
    }
}
