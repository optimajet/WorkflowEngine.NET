using System;
using System.Reflection;
using System.Xml.Linq;
using OptimaJet.Workflow.Core.Builder;
using OptimaJet.Workflow.Core.Parser;
using OptimaJet.Workflow.Core.Runtime;
using WF.Sample.Business.DataAccess;
using System.Threading.Tasks;
using System.Collections.Generic;
using OptimaJet.Workflow.Core.Model;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.Migrator;
using OptimaJet.Workflow.Plugins;
using OptimaJet.Workflow.Plugins.AssignmentsPlugin;
using WF.Sample.Business.Migrations;
using WF.Sample.Business.Model;

namespace WF.Sample.Business.Workflow
{
    public static class WorkflowInit
    {
        private static volatile WorkflowRuntime _runtime;

        private static readonly object _sync = new object();

        public static IDataServiceProvider DataServiceProvider { get; private set; }

        public static WorkflowRuntime Create(IDataServiceProvider dataServiceProvider)
        {
            DataServiceProvider = dataServiceProvider;
            CreateRuntime();
            return Runtime;
        }

        private static void CreateRuntime()
        {
            if (_runtime == null)
            {
                lock (_sync)
                {
                    if (_runtime == null)
                    {
                        var loopPlugin = new LoopPlugin();
                        var filePlugin = new FilePlugin();
                        var approvalPlugin = new ApprovalPlugin();
                        var assignmentPlugin = new AssignmentPlugin();
                        assignmentPlugin.OnDeadlineToCompleteAsync += CheckDeadlineToCompleteAsync;
                        assignmentPlugin.OnDeadlineToStartAsync += CheckDeadlineToStartAsync;

                        #region ApprovalPlugin Settings
                        
                        approvalPlugin.GetUserNamesByIds += GeUserNamesByIds;
                        // approvalPlugin.AutoApprovalHistory = true;
                        // approvalPlugin.NameParameterForComment = "Comment";

                        #endregion ApprovalPlugin Settings
                        
                        var basicPlugin = new BasicPlugin();
                        
                        #region BasicPlugin Settings
                        
                        //Settings for SendEmail actions
                        // basicPlugin.Setting_Mailserver = "smtp.yourserver.com";
                        // basicPlugin.Setting_MailserverPort = 25;
                        // basicPlugin.Setting_MailserverFrom = "from@yourserver.com";
                        // basicPlugin.Setting_MailserverLogin = "login@yourserver.com";
                        // basicPlugin.Setting_MailserverPassword = "password";
                        // basicPlugin.Setting_MailserverSsl = true;
                        
                        //not implemented
                        basicPlugin.ApproversInStageAsync += ApproversInStageAsync;
                        
                        basicPlugin.UsersInRoleAsync += UsersInRoleAsync;
                        basicPlugin.CheckPredefinedActorAsync += CheckPredefinedActorAsync;
                        basicPlugin.GetPredefinedIdentitiesAsync += GetPredefinedIdentitiesAsync;
                        basicPlugin.UpdateDocumentStateAsync += UpdateDocumentStateAsync;
                        
                        basicPlugin.WithActors(new List<string>() {"Manager", "Author"});
                        
                        #endregion BasicPlugin Settings
                        
                        var provider = DataServiceProvider.Get<IPersistenceProviderContainer>().Provider;
                        
                        var externalParametersProvider = new ExternalParametersProvider();
                        externalParametersProvider.GetDocument += GetDocument;

                        var builder = new WorkflowBuilder<XElement>(provider, new XmlWorkflowParser(), provider).WithDefaultCache();
                        _runtime = new WorkflowRuntime();

                        _runtime.GetUsersWithIdentitiesAsync += GetUsersWithIdentitiesAsync;
                        _runtime.GetUserByIdentityAsync += GetUserById;
                        _runtime.AssignmentApi.OnUpdateAssignment += OnUpdateAssignment;
                        _runtime.AssignmentApi.OnPostUpdateAssignment += OnPostUpdateAssignment;
                        _runtime.GetCustomTimerValueAsync += GetCustomTimerValueAsync;
                        _runtime.GetCustomTimerValueAsync += GetCustomTimerValueAsync;
                        
                        _runtime
                            .WithBuilder(builder)
                            .WithActionProvider(new ActionProvider(DataServiceProvider))
                            .WithRuleProvider(new RuleProvider(DataServiceProvider))
                            .WithDesignerAutocompleteProvider(new AutoCompleteProvider())
                            .WithPersistenceProvider(provider)
                            .SwitchAutoUpdateSchemeBeforeGetAvailableCommandsOn()
                            .RegisterAssemblyForCodeActions(Assembly.GetExecutingAssembly())
                            .WithPlugins(null, basicPlugin, loopPlugin, filePlugin, approvalPlugin, assignmentPlugin)
                            .WithExternalParametersProvider(externalParametersProvider)
                            .CodeActionsDebugOn()
                            .RunMigrations()
                            .RunCustomMigration(typeof(Migration2000010StructDivision).Assembly)
                            .AsSingleServer() //.AsMultiServer()
                        //    .WithConsoleAllLogger()
                            .Start();

                    }
                }
            }
        }
        
        public static async Task<DateTime?> GetCustomTimerValueAsync(string value, string name)
        {
            return value == "ten seconds" ? Runtime.RuntimeDateTimeNow.AddSeconds(10) : (DateTime?) null;
        }
        
        public static Assignment OnUpdateAssignment(Assignment newAssignment, Assignment oldAssignment)
        {
            if (newAssignment.StatusState == AssignmentPlugin.DefaultStartStatus &&
                oldAssignment.StatusState == AssignmentPlugin.DefaultStatus)
            {
                newAssignment.DateStart = DateTime.Now;
            }

            if (newAssignment.StatusState == AssignmentPlugin.DefaultCompletedStatus && oldAssignment.StatusState != AssignmentPlugin.DefaultCompletedStatus
                || 
                newAssignment.StatusState == AssignmentPlugin.DefaultDeclinedStatus && oldAssignment.StatusState != AssignmentPlugin.DefaultDeclinedStatus)
            {
                newAssignment.DateFinish = DateTime.Now;
            }

            return newAssignment;
        }
        
        public static Assignment OnPostUpdateAssignment(Assignment newAssignment, Assignment oldAssignment)
        {
            if (newAssignment.StatusState == AssignmentPlugin.DefaultCompletedStatus && oldAssignment.StatusState != AssignmentPlugin.DefaultCompletedStatus
                || 
                newAssignment.StatusState == AssignmentPlugin.DefaultDeclinedStatus && oldAssignment.StatusState != AssignmentPlugin.DefaultDeclinedStatus)
            {
                newAssignment.IsActive = false;
            }

            return newAssignment;
        }

        public static async Task CheckDeadlineToStartAsync(Assignment assignment, AssignmentCheckDeadlineCondition condition)
        {
            if (condition == AssignmentCheckDeadlineCondition.Equal && assignment.StatusState == AssignmentPlugin.DefaultStatus)
            {
                assignment.Tags.Add("Overdue start deadline");
                await Runtime.AssignmentApi.UpdateAssignmentAsync(assignment);
            }
        }
        public static async Task CheckDeadlineToCompleteAsync(Assignment assignment, AssignmentCheckDeadlineCondition condition)
        {
            if (condition == AssignmentCheckDeadlineCondition.Equal && (assignment.StatusState == AssignmentPlugin.DefaultStatus || assignment.StatusState == AssignmentPlugin.DefaultStartStatus))
            {
                assignment.Tags.Add("Overdue finish deadline");
                await Runtime.AssignmentApi.UpdateAssignmentAsync(assignment);
            }
        }
        
        public static async Task<string> GetUserById(string identity)
        {
            var provider = DataServiceProvider.Get<IEmployeeRepository>();
            var id = Guid.Parse(identity);
            return  provider.GetNameById(id);
        }
        
        public static async Task<IEnumerable<UserIdentity>> GetUsersWithIdentitiesAsync(string userName = null, SortDirection sortDirection = SortDirection.Asc, Paging paging = null)
        {
            var result = new List<UserIdentity>();
            var provider = DataServiceProvider.Get<IEmployeeRepository>();
            var users = provider.GetWithPaging(userName, sortDirection, paging);

            foreach (var user in users)
            {
                result.Add(new UserIdentity() {Identity = user.Id.ToString(), UserName = user.Name});
            }

            return result;
        }
        
        public static async Task<IEnumerable<string>> UsersInRoleAsync(string roleName, ProcessInstance processInstance)
        {
            var provider = DataServiceProvider.Get<IEmployeeRepository>();
            return  provider.GetInRole(roleName);
        }
        
        public static async Task<bool> CheckPredefinedActorAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter, string identityId)
        {
            var doc = GetDocument(processInstance);
            if (Guid.TryParse(identityId, out Guid id))
            {
                if (parameter == "Author")
                {
                    if (id == doc.AuthorId)
                        return true;
                    else
                        return false;
                }

                if (parameter == "Manager")
                {
                    if (id == doc.ManagerId)
                        return true;
                    else
                        return false;
                }
            }

            return false;
        }

        public static async Task<IEnumerable<string>> GetPredefinedIdentitiesAsync(ProcessInstance processInstance, WorkflowRuntime runtime, string parameter)
        {
            var doc = GetDocument(processInstance);
            
            if (parameter == "Author")
                return new List<string>() {doc.AuthorId.ToString()};
            
            if (parameter == "Manager")
                return new List<string>() {doc.ManagerId.ToString()};

            return new List<string>();
        }

        public static async Task<IEnumerable<string>> ApproversInStageAsync(string stageName, ProcessInstance processInstance)
        {
            throw new NotImplementedException();
        }
        public static List<string> GeUserNamesByIds(List<string> idS)
        {
            var employeeRepository = DataServiceProvider.Get<IEmployeeRepository>();
            List<string> names = new List<string>();
            foreach (var id in idS)
            {
                if(Guid.TryParse(id, out Guid guid))
                    names.Add(employeeRepository.GetNameById(guid));
                else
                    names.Add(id);
            }

            return names;
        }
        public static async Task UpdateDocumentStateAsync(ProcessInstance processInstance, string stateName, string  localizedStateName)
        {
            var docRepository = DataServiceProvider.Get<IDocumentRepository>();
            docRepository.ChangeState(processInstance.ProcessId, stateName, localizedStateName);
        }

        public static Document GetDocument(ProcessInstance processInstance)
        {
            var docRepository = DataServiceProvider.Get<IDocumentRepository>();
            var doc = docRepository.Get(processInstance.ProcessId);
            return doc;
        }
        
        public static WorkflowRuntime Runtime => _runtime;
        
    }
}
