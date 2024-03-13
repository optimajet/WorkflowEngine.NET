using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Model;
using WF.Sample.Models;

namespace WF.Sample.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsProvider _settingsProvider;

        public SettingsController(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public Task<ActionResult> Edit()
        {
            return Task.FromResult<ActionResult>(View(GetModel()));
        }

        #region Other
        private SettingsModel GetModel()
        {
            var model = new SettingsModel();
            model.SchemeName = "SimpleWF";

            var config = _settingsProvider.GetSettings();
            model.Employees = config.Employees;
            model.Roles = config.Roles;
            model.StructDivision = config.StructDivision;

            return model;
        }

        public static string GenerateColumnHtml(string name, StructDivision m, List<StructDivision> Model, List<Employee> employes, ref int index, string refId)
        {
            string valuePrefix = string.Format("{0}[{1}]", name, index);

            var sb = new StringBuilder();
            string trName = string.Format("tr_{0}{1}", name, index);

            sb.AppendFormat("<tr Id='{0}' {1}>", trName,
                            string.IsNullOrEmpty(refId) ? string.Empty : string.Format("class='child-of-{0}'", refId));
            sb.AppendFormat("<input type='hidden' name='{0}.Id' value='{1}'></input>", valuePrefix, m.Id);
            sb.AppendFormat("<input type='hidden' name='{0}.ParentId' value='{1}'></input>", valuePrefix, m.ParentId);
            sb.AppendFormat("<td class='columnTree'><b>{0}</b></td>", m.Name);
            sb.AppendFormat("<td></td>");
            sb.Append("</tr>");

            foreach (var item in employes.Where(c => c.StructDivisionId == m.Id))
            {
                index++;
                sb.Append(GenerateColumnHtml(name, item, ref index, trName));
            }

            foreach (var item in Model.Where(c => c.ParentId == m.Id))
            {
                index++;
                sb.Append(GenerateColumnHtml(name, item, Model, employes, ref index, trName));
            }

            return sb.ToString();
        }

        public static string GenerateColumnHtml(string name, Employee m, ref int index, string refId)
        {
            string valuePrefix = string.Format("{0}[{1}]", name, index);

            var sb = new StringBuilder();
            string trName = string.Format("tr_{0}{1}", name, index);

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
        #endregion
    }
}
