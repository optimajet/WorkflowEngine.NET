using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WF.Sample.Controls
{
    public partial class FoldersControl : UserControl
    {
        protected int Folder { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Folder = 0;
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.RawUrl.ToLower().Contains("inbox"))
                {
                    Folder = 1;
                }
                else if (HttpContext.Current.Request.RawUrl.ToLower().Contains("outbox"))
                {
                    Folder = 2;
                }
                else if (HttpContext.Current.Request.RawUrl.ToLower().Contains("assignments"))
                {
                    Folder = 3;
                }
                else if (HttpContext.Current.Request.RawUrl.ToLower().Contains("assignmentinfo"))
                {
                    Folder = 4;
                }
            }
        }
    }
}
