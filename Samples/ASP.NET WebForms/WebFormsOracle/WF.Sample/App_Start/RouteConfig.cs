using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;
using WF.Sample.Routing;

namespace WF.Sample
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute("DocumentListRoute", "Document/{action}", "~/Pages/Document/List.aspx", true, new RouteValueDictionary
            {
                { "action", "Index" }
            }, new RouteValueDictionary
            {
                { "action", "^.*Index.*$|^.*Inbox.*$|^.*Outbox.*$" }
            });

            routes.MapPageRoute("EditDocumentRoute", "Document/Edit/{id}", "~/Pages/Document/Edit.aspx", true, new RouteValueDictionary
            {
                { "id", Guid.Empty }
            });

            routes.Add(new Route("Document/DeleteRows", new HttpHandlerRoute("~/Pages/Document/DeleteRowsHandler.ashx")));

            routes.Add(new Route("Designer/API", new HttpHandlerRoute("~/Pages/Designer/WFEDesigner.ashx")));
            
            routes.MapPageRoute("Designer", "Designer", "~/Pages/Designer/Index.aspx");

            routes.MapPageRoute("Settings", "Settings/Edit", "~/Pages/Settings/Edit.aspx");       
            
        }
    }
}
