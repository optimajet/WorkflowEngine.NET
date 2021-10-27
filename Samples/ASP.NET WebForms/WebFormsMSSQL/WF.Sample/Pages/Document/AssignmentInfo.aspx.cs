using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class AssignmentInfo : Page
    {
        public IDocumentRepository DocumentRepository { get; set; }
        public IEmployeeRepository EmployeeRepository { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public bool ForCreate { get; set; } 
        public bool IsDeleted { get; set; } 
        public bool IsActive { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateFinish { get; set; }

        public AssignmentInfoModel GetAssignmentModel()
        {
            var id = RouteData.Values["assignmentId"] != null ? new Guid(RouteData.Values["assignmentId"].ToString()) : Guid.Empty;

            AssignmentInfoModel model = null;

            if(id!=Guid.Empty)
            {
                var assignment = WorkflowInit.Runtime.PersistenceProvider.GetAssignmentAsync(id).Result;
                if(assignment != null)
                {
                    model = GetAssignmentInfoModel(assignment, false);
                }
                ForCreate = false;
                IsDeleted = model.IsDeleted;
                IsActive = model.IsActive;
                DateStart = model.DateStart;
                DateFinish = model.DateFinish;
            }
            else
            {
                model = GetAssignmentInfoModel(new Assignment() {ProcessId =Guid.Parse(Request.QueryString["processId"])}, true);
                ForCreate = true;
                IsDeleted = false;
                IsActive = true;
            }

            return model;
        }
        
        public void UpdateAssignmentModel(Guid assignmentId)
        {
            var am = new AssignmentInfoModel();
            
            TryUpdateModel(am);
            if (!ModelState.IsValid)
            {
                return;
            }

            if (am.FormAction == "Create")
            {
                var id = Guid.NewGuid();
                var form = new AssignmentCreationForm()
                {
                    AssignmentCode = am.AssignmentCode, 
                    DeadlineToComplete = am.DeadlineToComplete == null ? (DateTime?) null : DateTime.Parse(am.DeadlineToComplete),
                    DeadlineToStart = am.DeadlineToStart == null ? (DateTime?) null : DateTime.Parse(am.DeadlineToStart),
                    Description = am.Description,
                    Executor = ((Guid)am.Executor).ToString(),
                    Id = id,
                    IsActive = true,
                    Name = am.Name,
                    Observers = am.Observers?.Select(x => x.Key.ToString()).ToList() ?? new List<string>() ,
                    Tags = am.Tags ?? new List<string>()
                };
                bool result = WorkflowInit.Runtime.AssignmentApi.CreateAssignmentAsync(am.ProcessId, form).Result;
                Response.Redirect($"~/Document/AssignmentInfo/{form.Id}");
            }
            else
            {
                var a = WorkflowInit.Runtime.PersistenceProvider.GetAssignmentAsync((Guid)am.AssignmentId).Result;
            
                a.Description = am.Description;
                a.Executor = am.Executor.ToString();
                a.Name = am.Name;
                a.StatusState = am.StatusState;
                a.DeadlineToComplete = am.DeadlineToComplete == null ? (DateTime?) null : DateTime.Parse(am.DeadlineToComplete);
                a.DeadlineToStart = am.DeadlineToStart == null ? (DateTime?) null : DateTime.Parse(am.DeadlineToStart);

                bool result = WorkflowInit.Runtime.AssignmentApi.UpdateAssignmentAsync(a).Result;
                Response.Redirect($"~/Document/AssignmentInfo/{am.AssignmentId}");
            }
        }
        
        protected string GetErrorStyle(string controlName)
        {
            return ModelState[controlName] != null && ModelState[controlName].Errors.Count > 0 ? "error" : "";
        }

        protected static string GetValue(string field)
        {
            return String.IsNullOrEmpty(field) ?  "-" : field;
        } 
        
        public Dictionary<string,string> GetStatuses()
        {
            var statuses = WorkflowInit.Runtime.AssignmentApi.GetAssignmentStatuses();
            statuses.Add("Any");
            return statuses.ToDictionary(e => e, e => e);;
        }
        
        public Dictionary<Guid,string> GetExecutors()
        {
            var employees = EmployeeRepository.GetAll();
            return employees.ToDictionary(e => e.Id, e => e.Name);
        }

        private AssignmentInfoModel GetAssignmentInfoModel(Assignment assignment, bool forCreate)
        {
            var document = DocumentRepository.Get(assignment.ProcessId);
            var employees = EmployeeRepository.GetAll();
            
            var am = new AssignmentInfoModel
            {
                AssignmentId = assignment.AssignmentId,
                Name = assignment.Name,
                StatusState = assignment.StatusState,
                ProcessId = assignment.ProcessId,
                DateCreation = assignment.DateCreation,
                DateStart = assignment.DateStart,
                DateFinish = assignment.DateFinish,
                DeadlineToComplete = assignment.DeadlineToComplete?.ToString("yyyy-MM-ddTHH:mm"),
                DeadlineToStart = assignment.DeadlineToStart?.ToString("yyyy-MM-ddTHH:mm"),
                Tags = assignment.Tags ?? new List<string>(),
                IsDeleted = assignment.IsDeleted,
                Description = assignment.Description,
                AssignmentCode = assignment.AssignmentCode,
                DocumentNumber = document?.Number,
            };
            
            if (forCreate)
            {
                am.Observers = new Dictionary<Guid, string>();
                am.ExecutorName = null;
                am.Executor = null;
                am.IsActive = true;
                am.FormAction = "Create";
            }
            else
            {
                var ids = assignment.Observers?.Select(Guid.Parse).Distinct().ToList();
                var observers = employees.Where(e => ids.Contains(e.Id)).Distinct()
                    .ToDictionary(e => e.Id, e=> e.Name);

                am.Observers = observers;
                am.Executor = Guid.Parse(assignment.Executor);
                am.ExecutorName = employees.FirstOrDefault(e => e.Id == Guid.Parse(assignment.Executor))?.Name;
                am.IsActive = assignment.IsActive;
                am.FormAction = "Update";
            }
            
            return am;
        }
      
    }
}
