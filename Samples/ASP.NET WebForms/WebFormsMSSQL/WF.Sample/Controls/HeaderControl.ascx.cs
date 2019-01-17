using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Extensions;
using WF.Sample.Helpers;

namespace WF.Sample.Controls
{
    public partial class HeaderControl : UserControl
    {
        public IEmployeeRepository EmployeeRepository { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            EmployeeList.Attributes["onchange"] = "CurrentEmployee_OnChange(this);";

            Guid currentEmployeeId = CurrentUserSettings.GetCurrentUser();

            if (currentEmployeeId == Guid.Empty)
            {
                IList<Employee> list = EmployeeRepository.GetAll();
                if (list != null && list.Count > 0)
                {
                    currentEmployeeId = list[0].Id;
                    CurrentUserSettings.SetUserInCookies(currentEmployeeId);
                }
            }

            EmployeeList.DataSource = GetData();
            EmployeeList.DataBind();
            EmployeeList.SelectedValue = currentEmployeeId.ToString();
        }

        protected ICollection GetData()
        {
            return EmployeeRepository.GetAll().Select(x => new
            {
                Text = $"Name: {x.Name}; StructDivision: {x.StructDivision.Name}; Roles: {x.GetListRoles()}",
                Value = x.Id.ToString()
            }).ToList();
        }
        
    }
}