using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.XtraReports.UI;

namespace Budget2.Server.BillDemand
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            wForm.GetForm = GetForm;
        }

        public XtraReport GetForm (Guid id)
        {
            Budget2.Server.BillDemand.PrintForm pf = new Budget2.Server.BillDemand.PrintForm();
            pf.SetBillDemand(id);
            return pf;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}