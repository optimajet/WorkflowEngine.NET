using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;

namespace WF.Sample.Pages.Settings
{
    public partial class Edit : Page
    {
        public ISettingsProvider SettingsProvider { get; set; }

        protected Business.Model.Settings Model { get; private set; }

        private int _structDivisionIndex = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            Model = SettingsProvider.GetSettings();

            RolesTableRepeater.DataBind();

            _structDivisionIndex = 0;
            StructDivisionRepeater.DataSource = Model.StructDivision.Where(c => !c.ParentId.HasValue);
            StructDivisionRepeater.DataBind();
        }

        public string GenerateColumnsHtml(string name, StructDivision m, string refId)
        {
            string valuePrefix = string.Format("{0}[{1}]", name, _structDivisionIndex);

            var sb = new StringBuilder();
            string trName = string.Format("tr_{0}{1}", name, _structDivisionIndex);

            sb.AppendFormat("<tr Id='{0}' {1}>", trName,
                            string.IsNullOrEmpty(refId) ? string.Empty : string.Format("class='child-of-{0}'", refId));
            sb.AppendFormat("<input type='hidden' name='{0}.Id' value='{1}'></input>", valuePrefix, m.Id);
            sb.AppendFormat("<input type='hidden' name='{0}.ParentId' value='{1}'></input>", valuePrefix, m.ParentId);
            sb.AppendFormat("<td class='columnTree'><b>{0}</b></td>", m.Name);
            sb.AppendFormat("<td></td>");
            sb.Append("</tr>");

            foreach (var item in Model.Employees.Where(c => c.StructDivisionId == m.Id))
            {
                _structDivisionIndex++;
                sb.Append(GenerateColumnHtml(name, item, trName));
            }

            foreach (var item in Model.StructDivision.Where(c => c.ParentId == m.Id))
            {
                _structDivisionIndex++;
                sb.Append(GenerateColumnsHtml(name, item, trName));
            }

            _structDivisionIndex += 1;
            return sb.ToString();
        }

        public string GenerateColumnHtml(string name, Employee m, string refId)
        {
            string valuePrefix = string.Format("{0}[{1}]", name, _structDivisionIndex);

            var sb = new StringBuilder();
            string trName = string.Format("tr_{0}{1}", name, _structDivisionIndex);

            sb.AppendFormat("<tr Id='{0}' {1}>", trName,
                            string.IsNullOrEmpty(refId) ? string.Empty : string.Format("class='child-of-{0}'", refId));
            sb.AppendFormat("<input type='hidden' name='{0}.Id' value='{1}'></input>", valuePrefix, m.Id);
            sb.AppendFormat("<td class='columnTree'>");
            sb.AppendFormat("{0}", m.Name);
            sb.AppendFormat("</td>");
            sb.AppendFormat("<td>");
            sb.AppendFormat("{0}", string.Join(",", m.EmployeeRoles.Select(c => c.Role.Name).ToArray()));
            sb.AppendFormat("</td>");
            sb.Append("</tr>");

            return sb.ToString();
        }
    }
}