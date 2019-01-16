using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

namespace WF.Sample.Routing
{
    public class HttpHandlerRoute : IRouteHandler
    {
        private string _virtualPath = null;

        public HttpHandlerRoute(string virtualPath)
        {
            _virtualPath = virtualPath;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            IHttpHandler httpHandler = (IHttpHandler)BuildManager.CreateInstanceFromVirtualPath(_virtualPath, typeof(IHttpHandler));
            return httpHandler;
        }
    }
}