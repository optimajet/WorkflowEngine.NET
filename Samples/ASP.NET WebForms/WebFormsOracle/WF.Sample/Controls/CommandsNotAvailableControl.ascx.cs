using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Models;

namespace WF.Sample.Controls
{
    public partial class CommandsNotAvailableControl : System.Web.UI.UserControl
    {
        public DocumentModel Model { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected bool Show()
        {
            return (Model.Commands.Count() == 0 && Model.HistoryModel.Items != null && 
                    Model.HistoryModel.Items.Any(c => !c.TransitionTime.HasValue));
        }
    }
}