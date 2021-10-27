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
            routes.MapPageRoute("DocumentListRoute", "Document/Index{action}", "~/Pages/Document/Index.aspx", true, new RouteValueDictionary
            {
                { "action", "Index" }
            });

            routes.MapPageRoute("DocumentInboxRoute", "Document/Inbox/{action}", "~/Pages/Document/Inbox.aspx", true,
            new RouteValueDictionary
            {
                {"action", "Inbox"}
            });
            
            routes.MapPageRoute("DocumentOutboxRoute", "Document/Outbox/{action}", "~/Pages/Document/Outbox.aspx", true,
            new RouteValueDictionary
            {
                {"action", "Outbox"}
            });
            
            routes.MapPageRoute("EditDocumentRoute", "Document/Edit/{id}", "~/Pages/Document/Edit.aspx", true, new RouteValueDictionary
            {
                { "id", Guid.Empty }
            });
            
            routes.MapPageRoute("DocumentAssignments", "Document/Assignments", "~/Pages/Document/Assignments.aspx", true);
            
            routes.MapPageRoute("DocumentAssignmentInfo", "Document/AssignmentInfo/{assignmentId}", "~/Pages/Document/AssignmentInfo.aspx", true,  new RouteValueDictionary
            {
                { "assignmentId", Guid.Empty }
            });
            
            routes.MapPageRoute("DocumentAssignmentCreate", "Document/AssignmentCreate/{processId}", "~/Pages/Document/AssignmentInfo.aspx", true,  new RouteValueDictionary
            {
                { "processId", Guid.Empty }
            });
            
            routes.Add(new Route("Document/DeleteRows", new HttpHandlerRoute("~/Pages/Document/DeleteRowsHandler.ashx")));
            
            routes.Add(new Route("Document/DeleteAssignments", new HttpHandlerRoute("~/Pages/Document/DeleteAssignmentsHandler.ashx")));

            routes.Add(new Route("Designer/API", new HttpHandlerRoute("~/Pages/Designer/WFEDesigner.ashx")));
            
            routes.MapPageRoute("Designer", "Designer", "~/Pages/Designer/Index.aspx");

            routes.MapPageRoute("Settings", "Settings/Edit", "~/Pages/Settings/Edit.aspx");       
            
        }
    }
}
