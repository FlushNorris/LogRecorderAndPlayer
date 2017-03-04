using System.Web;

namespace LogRecorderAndPlayer
{
    public class WebContext
    {
        public HttpContext HttpContext { get; set; }
    }

    public static class WebHelper
    {
        public static WebContext GetContext(HttpContext httpContext)
        {
            return new WebContext() {HttpContext = httpContext};
        }

        public static HttpContext ResumeContext(WebContext webContext, HttpContext httpContext)
        {
            if (httpContext != null)
                return httpContext;
            return webContext.HttpContext;
        }
    }
}
