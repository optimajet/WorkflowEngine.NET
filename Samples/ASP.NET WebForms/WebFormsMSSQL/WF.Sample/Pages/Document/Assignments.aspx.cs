using OptimaJet.Workflow.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Microsoft.Ajax.Utilities;
using OptimaJet.Workflow.Core.Persistence;
using WF.Sample.Business.DataAccess;
using WF.Sample.Business.Workflow;
using WF.Sample.Helpers;
using WF.Sample.Models;

namespace WF.Sample.Pages.Document
{
    public partial class Assignments : Page , IPaging
    {
        public IDocumentRepository DocumentRepository { get; set; }
        public IEmployeeRepository EmployeeRepository { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public int Count { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 15;
        protected int LastPage => (int) Math.Ceiling((double) Count /PageSize);

        public Dictionary<string, AssignmentFilter> GetFilters()
        {
            Guid identityId  = CurrentUserSettings.GetCurrentUser();
            return GenerateFilters(identityId.ToString());
        }

        public AssignmentFilterModel GetFilterModel()
        {
            if (IsPostBack)
            {
                return GetAssignmentsWithCustomFilter();
            }
            
            var filter =  new AssignmentFilter();
            int count = WorkflowInit.Runtime.PersistenceProvider.GetAssignmentCountAsync(filter).Result;
            List<Assignment> assignments = WorkflowInit.Runtime.AssignmentApi.GetAssignmentsAsync(filter, new Paging(1, PageSize)).Result;

            Count = count;
            PageNumber = 1;
            AssignmentsTableRepeater.DataSource = GetAssignmentModels(assignments);
            AssignmentsTableRepeater.DataBind();
                
            return new AssignmentFilterModel();
        }

        private AssignmentFilterModel GetAssignmentsWithCustomFilter()
        {
             var model = new AssignmentFilterModel()
                {
                    FilterName = Request.Form[$"{Master.UniqueID}$MainContent$formFilter$FilterName"],
                    AssignmentCode = Request.Form[$"{Master.UniqueID}$MainContent$formFilter$AssignmentCode"],
                    StatusState = Request.Form[$"{Master.UniqueID}$MainContent$formFilter$StatusState"],
                    Page = Int32.Parse(Request.Form[$"{Master.UniqueID}$MainContent$formFilter$Page"]),
                };
                
                var isCorrect = Int32.TryParse(Request.Form[$"{Master.UniqueID}$MainContent$formFilter$DocumentNumber"], out int documentNumber);

                model.DocumentNumber = isCorrect ? documentNumber : (int?) null;

                Guid identityId  = CurrentUserSettings.GetCurrentUser();
                var assignmentFilters = GenerateFilters(identityId.ToString());
                
                AssignmentFilter customFilter;
            
                if (assignmentFilters.TryGetValue(model.FilterName, out var res))
                {
                    customFilter = res;
                
                    if (model.DocumentNumber != null)
                    {
                        var doc = DocumentRepository.GetByNumber((int)model.DocumentNumber);
                        if (doc != null)
                        {
                            customFilter.ProcessIdEqual(doc.Id);
                        }
                        else
                        {
                            customFilter.ProcessIdEqual(Guid.Empty);
                        }
                    }
                    if (!model.AssignmentCode.IsNullOrWhiteSpace())
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
                    model = new AssignmentFilterModel(){Page = model?.Page ?? 1};
                }

                int count = WorkflowInit.Runtime.PersistenceProvider.GetAssignmentCountAsync(customFilter).Result;

                model.Page = model?.Page ?? 1;
                var maxPage = Math.Ceiling(count / (decimal)PageSize);
                if (model.Page > maxPage)
                {
                    model.Page = (int)maxPage;
                }
            
                List<Assignment> assignments = WorkflowInit.Runtime.AssignmentApi.GetAssignmentsAsync(customFilter, new Paging(model.Page, PageSize)).Result;

                Count = count;
                PageNumber = model.Page;
                AssignmentsTableRepeater.DataSource = GetAssignmentModels(assignments);
                AssignmentsTableRepeater.DataBind();

                return model;
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
            
            var documents = DocumentRepository.GetByIds(ids).Distinct()
                .ToDictionary(x=>x.Id, x=>x);
            
            var assignmentModels = new List<AssignmentItemModel>();
            
            foreach (var a in assignments)
            {
                documents.TryGetValue(a.ProcessId, out Business.Model.Document doc);
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
                    ExecutorName = EmployeeRepository.GetNameById(Guid.Parse(a.Executor))
                };
                
                assignmentModels.Add(am);
            }
            
            return assignmentModels;
        }
        
        public IEnumerable<string> GetStatuses()
        {
            var statuses = WorkflowInit.Runtime.AssignmentApi.GetAssignmentStatuses();
            statuses.Add("Any");
            return statuses;
        }

        protected string GetAssignmentStateStyle(AssignmentItemModel item)
        {
            if (item.IsDeleted) {
                return "color: red";
            }
            return item.IsActive ? "color: green" : "color: orange";
        }

        protected string GetAssignmentStateText(AssignmentItemModel item)
        {
            if (item.IsDeleted) { 
                return "Deleted";
            }

            return item.IsActive ? "Active" : "Not Active";
        }
    }
}
