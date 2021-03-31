using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Models;

namespace WF.Sample.Controls
{
    public partial class Paging : System.Web.UI.UserControl
    {
        public IPaging Model { get; set; }
        public int LastPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            base.DataBind();
            LastPage = (int) Math.Ceiling((double) Model.Count / Model.PageSize);
        }
        
        protected string GetActionName()
        {
            var action = this.Page.RouteData.Values["action"]?.ToString() ?? "Index";
            return action == "Index" ? string.Empty : "/Document/" + action;
        }
    }
}
