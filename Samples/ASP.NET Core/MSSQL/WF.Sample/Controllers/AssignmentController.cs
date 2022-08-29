using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WF.Sample.Business.Model;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private int pageSize = 15;
        public AssignmentController(IDocumentRepository documentRepository, IEmployeeRepository employeeRepository)
        {
            _documentRepository = documentRepository;
            _employeeRepository = employeeRepository;
        }
        
        public async Task<ActionResult> Assignments(AssignmentFilterModel model = null)
        {
            var identityId = CurrentUserSettings.GetCurrentUser(HttpContext).ToString();

            AssignmentFilter customFilter;

            var assignmentFilters = GenerateFilters(identityId);

            if (model != null && assignmentFilters.TryGetValue(model.FilterName, out var res))
            {
                customFilter = res;
                
                if (model.DocumentNumber != null)
                {
                    var doc = _documentRepository.GetByNumber((int)model.DocumentNumber);
                    if (doc != null)
                    {
                        customFilter.ProcessIdEqual(doc.Id);
                    }
                    else
                    {
                        customFilter.ProcessIdEqual(Guid.Empty);
                    }
                }
                if (model.AssignmentCode != null)
                {
                    customFilter.CodeEquals(model.AssignmentCode);
                }
                if (model.StatusState != "Any")
                {
                    customFilter.StatusStateEquals(model.StatusState);
                }
            }
            else
            {
                customFilter = new AssignmentFilter();
                model = new AssignmentFilterModel(){Page = model?.Page ?? 1, PageChanged = 0};
            }
            
            int count = await WorkflowInit.Runtime.PersistenceProvider.GetAssignmentCountAsync(customFilter);

            model.Page = model?.Page ?? 1;
            var maxPage = Math.Ceiling(count / (decimal)pageSize);
            if (model.Page > maxPage)
            {
                model.Page = (int)maxPage;
            }
            
            List<Assignment> assignments = await WorkflowInit.Runtime.AssignmentApi.GetAssignmentsAsync(customFilter,new Paging(model.Page, pageSize));

            var statuses = WorkflowInit.Runtime.AssignmentApi.GetAssignmentStatuses();
            statuses.Add("Any");
                
            return View("Assignments", new AssignmentListModel()
            {
                Assignments = GetAssignmentModels(assignments),
                PageSize = pageSize,
                Count = count,
                Filters = assignmentFilters,
                CustomFilter = model,
                Statuses = statuses
            });
        }
        
        public async Task<ActionResult> AssignmentInfo(Guid id)
        {
            var assignment = await WorkflowInit.Runtime.PersistenceProvider.GetAssignmentAsync(id);
            
            return View("AssignmentInfo", await GetAssignmentInfoModel(assignment, false) );
        }
        
        public async Task<ActionResult> AssignmentCreate(Guid processid)
        {
            var assignment = new Assignment()
            {
                ProcessId = processid
            };

            var a = await GetAssignmentInfoModel(assignment, true);
            return View("AssignmentInfo", a );
        }
        
        [HttpPost]
        public async Task<ActionResult> AssignmentCreateOrUpdate(AssignmentInfoModel am)
        {
            if (am.FormAction == "Create")
            {
                var id = Guid.NewGuid();
                var form = new AssignmentCreationForm()
                {
                    AssignmentCode = am.AssignmentCode, 
                    DeadlineToComplete = am.DeadlineToComplete,
                    DeadlineToStart = am.DeadlineToStart,
                    Description = am.Description,
                    Executor = ((Guid)am.Executor).ToString(),
                    Id = id,
                    IsActive = true,
                    Name = am.Name,
                    Observers = am.Observers?.Select(x => x.Key.ToString()).ToList() ?? new List<string>() ,
                    Tags = am.Tags ?? new List<string>()
                };

                bool result = await WorkflowInit.Runtime.AssignmentApi.CreateAssignmentAsync(am.ProcessId, form);
                return RedirectToAction("AssignmentInfo",new {id = id} );
            }
            else
            {
                var a = await WorkflowInit.Runtime.PersistenceProvider.GetAssignmentAsync((Guid)am.AssignmentId);
            
                a.Description = am.Description;
                a.Executor = am.Executor.ToString();
                a.Name = am.Name;
                a.StatusState = am.StatusState;
                a.DeadlineToComplete = am.DeadlineToComplete;
                a.DeadlineToStart = am.DeadlineToStart;

                bool result = await WorkflowInit.Runtime.AssignmentApi.UpdateAssignmentAsync(a);

                return RedirectToAction("AssignmentInfo",new {id = a.AssignmentId} );
            }
        }
        
        public async Task<ActionResult> DeleteAssignments(Guid[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Content("Items not selected");

            try
            {
                foreach (var id in ids)
                {
                    var result = await WorkflowInit.Runtime.AssignmentApi.DeleteAssignmentAsync(id);
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            
            return RedirectToAction("Assignments");
        }

        [HttpPost]
        public ActionResult AssignmentsFilter(AssignmentFilterModel model)
        {
            if (model.PageChanged == 0)
            {
                model.Page = 1;
            }
            else
            {
                model.PageChanged = 0;
            }
            
            return RedirectToAction("Assignments", model);
        }
        
        
        private static Dictionary<string, AssignmentFilter> GenerateFilters(string identityId)
        {
            var assignmentFilters = new Dictionary<string,AssignmentFilter>();
            var filterByExecutor = new AssignmentFilter().ExecutorEquals(identityId);
            
            var filterByExecutorAndActive = new AssignmentFilter().ExecutorEquals(identityId).IsDeleted(false).IsActive(true);

            var filterByObserver = new AssignmentFilter().IsObserver(identityId).IsDeleted(false);
            
            assignmentFilters.Add("All assigned to me", filterByExecutor);
            assignmentFilters.Add("Active and assigned to me", filterByExecutorAndActive);
            assignmentFilters.Add("All", new AssignmentFilter());
            assignmentFilters.Add("I am observer", filterByObserver);

            return assignmentFilters;
        }
        
        private List<AssignmentItemModel> GetAssignmentModels(List<Assignment> assignments)
        {
            var ids = assignments.Select(x => x.ProcessId).Distinct().ToList();
            
            var documents = _documentRepository.GetByIds(ids)
                .ToDictionary(x=>x.Id, x=>x);
            
            var assignmentModels = new List<AssignmentItemModel>();
            
            foreach (var a in assignments)
            {
                documents.TryGetValue(a.ProcessId, out Document doc);
                var am = new AssignmentItemModel
                {
                    
                    AssignmentId = a.AssignmentId,
                    Name = a.Name,
                    Executor = a.Executor,
                    StatusState = a.StatusState,
                    ProcessId = a.ProcessId,
                    DateCreation = a.DateCreation,
                    IsDeleted = a.IsDeleted,
                    IsActive = a.IsActive,
                    AssignmentCode = a.AssignmentCode,
                    DocumentNumber = doc?.Number,
                    ExecutorName = _employeeRepository.GetNameById(Guid.Parse(a.Executor))
                };
                
                assignmentModels.Add(am);
            }
            
            return assignmentModels;
        }
        
        private Task<AssignmentInfoModel> GetAssignmentInfoModel(Assignment assignment, bool forCreate)
        {
            var document = _documentRepository.Get(assignment.ProcessId);
            var employees = _employeeRepository.GetAll();
            
            var am = new AssignmentInfoModel
            {
                AssignmentId = assignment.AssignmentId,
                Name = assignment.Name,
                StatusState = assignment.StatusState,
                ProcessId = assignment.ProcessId,
                DateCreation = assignment.DateCreation,
                DateStart = assignment.DateStart,
                DateFinish = assignment.DateFinish,
                DeadlineToComplete = assignment.DeadlineToComplete,
                DeadlineToStart = assignment.DeadlineToStart,
                Tags = assignment.Tags ?? new List<string>(),
                IsDeleted = assignment.IsDeleted,
                Description = assignment.Description,
                AssignmentCode = assignment.AssignmentCode,
                DocumentNumber = document?.Number,
                Statuses = WorkflowInit.Runtime.AssignmentApi.GetAssignmentStatuses(),
                Employees = employees.ToDictionary(e => e.Id, e => e.Name)
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
                var observers = employees.Where(e => ids.Contains(e.Id))
                    .ToDictionary(e => e.Id, e=> e.Name);

                am.Observers = observers;
                am.Executor = Guid.Parse(assignment.Executor);
                am.ExecutorName = employees.FirstOrDefault(e => e.Id == Guid.Parse(assignment.Executor))?.Name;
                am.IsActive = assignment.IsActive;
                am.FormAction = "Update";
            }
            
            return Task.FromResult(am);
        }
    }
}
