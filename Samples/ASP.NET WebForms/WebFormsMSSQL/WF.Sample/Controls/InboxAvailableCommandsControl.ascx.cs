using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Models;
using WF.Sample.Pages.Document;

namespace WF.Sample.Controls
{
    public partial class InboxAvailableCommandsControl : UserControl
    {
        public InboxDocumentModel Model { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CommandsRepeater.DataSource = Model.AvailableCommands;
            CommandsRepeater.DataBind();
        }
        
        protected void ExecuteCommand(object sender, EventArgs e)
        {
            Edit.ExecuteCommand(Model.Id, (sender as Button).CommandName, Model);
            Page.Response.Redirect(Page.Request.Url.ToString(), true);
        }
    }
}
