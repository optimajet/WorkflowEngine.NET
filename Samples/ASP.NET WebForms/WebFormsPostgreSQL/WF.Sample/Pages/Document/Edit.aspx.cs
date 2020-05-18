using AutoMapper;
using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;
using WF.Sample.Extensions;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class Edit : Page
    {
        public IDocumentRepository DocumentRepository { get; set; }
        public IEmployeeRepository EmployeeRepository { get; set; }

        private string _buttonName;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public DocumentModel GetModel()
        {
            var id = RouteData.Values["id"] != null ? new Guid(RouteData.Values["id"].ToString()) : Guid.Empty;

            DocumentModel model = null;

            if (id != Guid.Empty)
            {
                var d = DocumentRepository.Get(id);
                if (d != null)
                {
                    CreateWorkflowIfNotExists(id);

                    var h = DocumentRepository.GetHistory(id);
                    model = new DocumentModel()
                    {
                        Id = d.Id,
                        AuthorId = d.AuthorId,
                        AuthorName = d.Author.Name,
                        Comment = d.Comment,
                        ManagerId = d.ManagerId,
                        ManagerName = d.ManagerId.HasValue ? d.Manager.Name : string.Empty,
                        Name = d.Name,
                        Number = d.Number,
                        StateName = d.StateName,
                        Sum = d.Sum,
                        Commands = GetCommands(id),
                        AvailiableStates = GetStates(id),
                        HistoryModel = new DocumentHistoryModel { Items = h }
                    };
                    model.StateNameToSet = model.AvailiableStates.Keys.FirstOrDefault();
                }

            }
            else
            {
                Guid userId = CurrentUserSettings.GetCurrentUser();
                model = new DocumentModel()
                {
                    AuthorId = userId,
                    AuthorName = EmployeeRepository.GetNameById(userId),
                    StateName = "Vacation request created"
                };
            }

            return model;

        }

        #region Workflow
        private DocumentCommandModel[] GetCommands(Guid id)
        {
            var result = new List<DocumentCommandModel>();
            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, CurrentUserSettings.GetCurrentUser().ToString());
            foreach (var workflowCommand in commands)
            {
                if (result.Count(c => c.key == workflowCommand.CommandName) == 0)
                    result.Add(new DocumentCommandModel() { key = workflowCommand.CommandName, value = workflowCommand.LocalizedName, Classifier = workflowCommand.Classifier });
            }
            return result.ToArray();
        }

        private Dictionary<string, string> GetStates(Guid id)
        {

            var result = new Dictionary<string, string>();
            var states = WorkflowInit.Runtime.GetAvailableStateToSet(id);
            foreach (var state in states)
            {
                if (!result.ContainsKey(state.Name))
                    result.Add(state.Name, state.VisibleName);
            }
            return result;

        }

        private void ExecuteCommand(Guid id, string commandName, DocumentModel document)
        {
            var currentUser = CurrentUserSettings.GetCurrentUser().ToString();

            if (commandName.Equals("SetState", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(document.StateNameToSet))
                    return;

                
                var setStateParams = new SetStateParams(id,document.StateNameToSet)
                {
                    IdentityId = currentUser,
                    ImpersonatedIdentityId = currentUser
                }.AddTemporaryParameter("Comment",document.Comment);
                WorkflowInit.Runtime.SetState(setStateParams);
                
                return;
            }

            var commands = WorkflowInit.Runtime.GetAvailableCommands(id, currentUser);

            var command =
                commands.FirstOrDefault(
                    c => c.CommandName.Equals(commandName, StringComparison.CurrentCultureIgnoreCase));

            if (command == null)
                return;

            if (command.Parameters.Count(p => p.ParameterName == "Comment") == 1)
                command.Parameters.Single(p => p.ParameterName == "Comment").Value = document.Comment ?? string.Empty;

            WorkflowInit.Runtime.ExecuteCommand(command, currentUser, currentUser);
        }

        private void CreateWorkflowIfNotExists(Guid id)
        {
            if (WorkflowInit.Runtime.IsProcessExists(id))
                return;

            WorkflowInit.Runtime.CreateInstance("SimpleWF", id);
        }

        #endregion

        // The id parameter name should match the DataKeyNames value set on the control
        public void UpdateDocumentModel(Guid id)
        {
            DocumentModel model = new DocumentModel();
            
            TryUpdateModel(model);
            if (ModelState.IsValid)
            {

                var doc = Mapper.Map<Business.Model.Document>(model);

                try
                {
                    doc = DocumentRepository.InsertOrUpdate(doc);

                    if (doc == null)
                    {
                        ModelState.AddModelError("", "Row not found!");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    var sb = new StringBuilder("Save error. " + ex.Message);
                    if (ex.InnerException != null)
                        sb.AppendLine(ex.InnerException.Message);
                    ModelState.AddModelError("", sb.ToString());
                    return;
                }

                if (_buttonName == "SaveAndExit")
                {
                    Response.Redirect("~/");
                    return;
                }
                 
                if (_buttonName != "Save")
                {
                    ExecuteCommand(doc.Id, _buttonName, model);
                }

                Response.Redirect($"~/Document/Edit/{doc.Id}");
            }
        }

        protected void ProcessButtonClick(object sender, EventArgs e)
        {
            _buttonName = (sender as Button).CommandArgument;
        }

        protected void ProcessCommandButtonClick(object sender, EventArgs e)
        {
            _buttonName = (sender as Button).CommandArgument;
            DocumentFormView.UpdateItem(true);
        }

        protected string GetErrorStyle(string controlName)
        {
            return ModelState[controlName] != null && ModelState[controlName].Errors.Count > 0 ? "error" : "";
        }

        protected bool ShowCommandsNotAvailableBlock(DocumentModel model)
        {
            if (model.Commands.Count() > 0 || model.AvailiableStates.Count > 0)
            {
                return (model.Commands.Count() == 0 && model.HistoryModel.Items.Any(c => !c.TransitionTime.HasValue));
            }

            return false;
        }

        protected string GetCommandButtonClass(TransitionClassifier classifier)
        {
            if (classifier == TransitionClassifier.Direct)
            {
                return "ui primary button";
            }
            else if (classifier == TransitionClassifier.Reverse)
            {
                return "ui secondary button";
            }
            return "ui floated button";
        }

        public ICollection GetEmployees()
        {
            
            return EmployeeRepository.GetAll().Select(x => new
            {
                Text = $"Name: {x.Name}; StructDivision: {x.StructDivision.Name}; Roles: {x.GetListRoles()}",
                Value = x.Id.ToString()
            }).ToList();
        }

        protected void DocumentFormView_ItemUpdating(object sender, FormViewUpdateEventArgs e)
        {
            if ((string)e.CommandArgument == "SetState")
            {
                var view = sender as FormView;
                var drop = view.FindControl("StateDropDownList") as DropDownList;
                e.NewValues["StateNameToSet"] = drop.SelectedValue;
            }
        }

        protected string GetDesignerUrl(Guid id)
        {
            var designerUrl = "~/Designer?schemeName=SimpleWF";
            if (id != Guid.Empty)
            {
                designerUrl = "~/Designer?processid=" + id.ToString();
            }
            return designerUrl;
        }
    }
}
