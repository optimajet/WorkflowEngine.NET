using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using OptimaJet.Workflow.Core.Generator;
using OptimaJet.Workflow.Core.Persistence;
using OptimaJet.Workflow.MongoDB;
using WF.Sample.Business.DataAccess;
using WF.Sample.MongoDb.Entities;

namespace WF.Sample.MongoDb.Implementation
{
  public class PersistenceProviderContainer : IPersistenceProviderContainer
  {
    private readonly MongoDBProvider _provider;

    public PersistenceProviderContainer()
    {
      var mongoClient = new MongoClient(new MongoUrl(ConfigurationManager.AppSettings["Url"]));
      _provider = new MongoDBProvider(mongoClient.GetDatabase(ConfigurationManager.AppSettings["Database"]));
      Provider = _provider;

      if (_provider.Store.GetCollection<Business.Model.Role>("Role").CountDocuments(new BsonDocument()) == 0)
      {
        GenerateData();
      }
    }


    public IWorkflowProvider Provider { get; private set; }

    private void GenerateData()
    {
      var roles = new List<Business.Model.Role>()
      {
        new Business.Model.Role() {Id = new Guid("8D378EBE-0666-46B3-B7AB-1A52480FD12A"), Name = "Big Boss"},
        new Business.Model.Role() {Id = new Guid("412174C2-0490-4101-A7B3-830DE90BCAA0"), Name = "Accountant"},
        new Business.Model.Role() {Id = new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), Name = "User"}
      };

      var sd = new List<Business.Model.StructDivision>()
      {
        new Business.Model.StructDivision() {Id = new Guid("B14F5D81-5B0D-4ACC-92B8-27CBBE39086B"), Name = "Group 1", ParentId = new Guid("F6E34BDF-B769-42DD-A2BE-FEE67FAF9045")},
        new Business.Model.StructDivision()
          {Id = new Guid("7E9FD972-C775-4C6B-9D91-47E9397BD2E6"), Name = "Group 1.1", ParentId = new Guid("B14F5D81-5B0D-4ACC-92B8-27CBBE39086B")},
        new Business.Model.StructDivision()
          {Id = new Guid("DC195A4F-46F9-41B2-80D2-77FF9C6269B7"), Name = "Group 1.2", ParentId = new Guid("B14F5D81-5B0D-4ACC-92B8-27CBBE39086B")},
        new Business.Model.StructDivision()
          {Id = new Guid("C5DCC148-9C0C-45C4-8A68-901D99A26184"), Name = "Group 2.2", ParentId = new Guid("72D461B2-234B-40D6-B410-B261964BA291")},
        new Business.Model.StructDivision() {Id = new Guid("72D461B2-234B-40D6-B410-B261964BA291"), Name = "Group 2", ParentId = new Guid("F6E34BDF-B769-42DD-A2BE-FEE67FAF9045")},
        new Business.Model.StructDivision()
          {Id = new Guid("BC21A482-28E7-4951-8177-E57813A70FC5"), Name = "Group 2.1", ParentId = new Guid("72D461B2-234B-40D6-B410-B261964BA291")},
        new Business.Model.StructDivision() {Id = new Guid("F6E34BDF-B769-42DD-A2BE-FEE67FAF9045"), Name = "Head Group"}
      };

      var e1 = new Employee()
      {
        Id = new Guid("E41B48E3-C03D-484F-8764-1711248C4F8A"),
        Name = "Maria",
        StructDivisionId = new Guid("C5DCC148-9C0C-45C4-8A68-901D99A26184"),
        IsHead = false
      };

      e1.Roles.Add(new Guid("412174C2-0490-4101-A7B3-830DE90BCAA0"), "Accountant");
      e1.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");

      var e2 = new Employee()
      {
        Id = new Guid("BBE686F8-8736-48A7-A886-2DA25567F978"),
        Name = "Mark",
        StructDivisionId = new Guid("7E9FD972-C775-4C6B-9D91-47E9397BD2E6"),
        IsHead = false,
      };
      e2.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");

      var e3 = new Employee()
      {
        Id = new Guid("81537E21-91C5-4811-A546-2DDDFF6BF409"),
        Name = "Silviya",
        StructDivisionId = new Guid("F6E34BDF-B769-42DD-A2BE-FEE67FAF9045"),
        IsHead = true,
      };
      e3.Roles.Add(new Guid("8D378EBE-0666-46B3-B7AB-1A52480FD12A"), "Big Boss");
      e3.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");

      var e4 = new Employee()
      {
        Id = new Guid("B0E6FD4C-2DB9-4BB6-A62E-68B6B8999905"),
        Name = "Margo",
        StructDivisionId = new Guid("DC195A4F-46F9-41B2-80D2-77FF9C6269B7"),
        IsHead = false,
      };
      e4.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");

      var e5 = new Employee()
      {
        Id = new Guid("DEB579F9-991C-4DB9-A17D-BB1ECCF2842C"),
        Name = "Max",
        StructDivisionId = new Guid("B14F5D81-5B0D-4ACC-92B8-27CBBE39086B"),
        IsHead = true,
      };
      e5.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");

      var e6 = new Employee()
      {
        Id = new Guid("91F2B471-4A96-4AB7-A41A-EA4293703D16"),
        Name = "John",
        StructDivisionId = new Guid("7E9FD972-C775-4C6B-9D91-47E9397BD2E6"),
        IsHead = true,
      };

      e6.Roles.Add(new Guid("71FFFB5B-B707-4B3C-951C-C37FDFCC8DFB"), "User");
      var employees = new List<Employee>() {e1, e2, e3, e4, e5, e6};


      var dbcollRole = _provider.Store.GetCollection<Business.Model.Role>("Role");
      foreach (var r in roles)
      {
        if (dbcollRole.Find(x => x.Id == r.Id).FirstOrDefault() == null)
          dbcollRole.InsertOne(r);
      }

      var dbcollStructDivision = _provider.Store.GetCollection<Business.Model.StructDivision>("StructDivision");
      foreach (var e in sd)
      {
        if (dbcollStructDivision.Find(x => x.Id == e.Id).FirstOrDefault() == null)
          dbcollStructDivision.InsertOne(e);
      }

      var dbcollEmployee = _provider.Store.GetCollection<Employee>("Employee");
      foreach (var e in employees)
      {
        if (dbcollEmployee.Find(x => x.Id == e.Id).FirstOrDefault() == null)
        {
          e.StructDivisionName = sd.Where(c => c.Id == e.StructDivisionId).First().Name;
          dbcollEmployee.InsertOne(e);
        }
      }

      var dbcollWorkflowScheme = _provider.Store.GetCollection<WorkflowScheme>("WorkflowScheme");

      if (dbcollWorkflowScheme.Find(x => x.Id == "SimpleWF").FirstOrDefault() == null)
      {
        #region Insert scheme

        dbcollWorkflowScheme.InsertOne(new WorkflowScheme()
        {
          Id = "SimpleWF",
          Code = "SimpleWF",
          Scheme = @"<Process Name=""SimpleWF"">
  <Designer X=""-110"" Y=""-60"" />
  <Actors>
    <Actor Name=""Author"" Rule=""IsDocumentAuthor"" Value="""" />
    <Actor Name=""Manager"" Rule=""IsDocumentManager"" Value="""" />
    <Actor Name=""BigBoss"" Rule=""CheckRole"" Value=""Big Boss"" />
    <Actor Name=""Accountant"" Rule=""CheckRole"" Value=""Accountant"" />
  </Actors>
  <Parameters>
    <Parameter Name=""Comment"" Type=""String"" Purpose=""Temporary"" />
  </Parameters>
  <Commands>
    <Command Name=""StartSigning"">
      <InputParameters>
        <ParameterRef Name=""Comment"" IsRequired=""false"" DefaultValue="""" NameRef=""Comment"" />
      </InputParameters>
    </Command>
    <Command Name=""Approve"">
      <InputParameters>
        <ParameterRef Name=""Comment"" IsRequired=""false"" DefaultValue="""" NameRef=""Comment"" />
      </InputParameters>
    </Command>
    <Command Name=""Reject"">
      <InputParameters>
        <ParameterRef Name=""Comment"" IsRequired=""false"" DefaultValue="""" NameRef=""Comment"" />
      </InputParameters>
    </Command>
    <Command Name=""Paid"">
      <InputParameters>
        <ParameterRef Name=""Comment"" IsRequired=""false"" DefaultValue="""" NameRef=""Comment"" />
      </InputParameters>
    </Command>
  </Commands>
  <Timers>
    <Timer Name=""SendToBigBoss"" Type=""Interval"" Value=""10minutes"" NotOverrideIfExists=""false"" />
  </Timers>
 <Activities>
     <Activity Name=""VacationRequestCreated"" State=""VacationRequestCreated"" IsInitial=""True"" IsFinal=""False"" IsForSetState=""True"" IsAutoSchemeUpdate=""True"">
       <Implementation>
         <ActionRef Order=""1"" NameRef=""UpdateTransitionHistory"" />
       </Implementation>
       <PreExecutionImplementation>
         <ActionRef Order=""1"" NameRef=""WriteTransitionHistory"" />
       </PreExecutionImplementation>
       <Designer X=""10"" Y=""170"" />
     </Activity>
     <Activity Name=""ManagerSigning"" State=""ManagerSigning"" IsInitial=""False"" IsFinal=""False"" IsForSetState=""True"" IsAutoSchemeUpdate=""True"">
       <Implementation>
         <ActionRef Order=""1"" NameRef=""UpdateTransitionHistory"" />
       </Implementation>
       <PreExecutionImplementation>
         <ActionRef Order=""1"" NameRef=""WriteTransitionHistory"" />
       </PreExecutionImplementation>
       <Designer X=""361.53846153846166"" Y=""172.69347319347324"" />
     </Activity>
     <Activity Name=""BigBossSigning"" State=""BigBossSigning"" IsInitial=""False"" IsFinal=""False"" IsForSetState=""True"" IsAutoSchemeUpdate=""True"">
       <Implementation>
         <ActionRef Order=""1"" NameRef=""UpdateTransitionHistory"" />
       </Implementation>
       <PreExecutionImplementation>
         <ActionRef Order=""1"" NameRef=""WriteTransitionHistory"" />
       </PreExecutionImplementation>
       <Designer X=""721.5384615384614"" Y=""172.6934731934732"" />
     </Activity>
     <Activity Name=""AccountingReview "" State=""AccountingReview "" IsInitial=""False"" IsFinal=""False"" IsForSetState=""True"" IsAutoSchemeUpdate=""True"">
       <Implementation>
         <ActionRef Order=""1"" NameRef=""UpdateTransitionHistory"" />
       </Implementation>
       <PreExecutionImplementation>
         <ActionRef Order=""1"" NameRef=""WriteTransitionHistory"" />
       </PreExecutionImplementation>
       <Designer X=""718.2051282051282"" Y=""334.3601398601398"" />
     </Activity>
     <Activity Name=""RequestApproved"" State=""RequestApproved"" IsInitial=""False"" IsFinal=""True"" IsForSetState=""True"" IsAutoSchemeUpdate=""True"">
       <Implementation>
         <ActionRef Order=""1"" NameRef=""UpdateTransitionHistory"" />
       </Implementation>
       <PreExecutionImplementation>
         <ActionRef Order=""1"" NameRef=""WriteTransitionHistory"" />
       </PreExecutionImplementation>
       <Designer X=""1036.5384615384614"" Y=""334.3601398601398"" />
     </Activity>
   </Activities>
   <Transitions>
     <Transition Name=""ManagerSigning_Draft_1"" To=""VacationRequestCreated"" From=""ManagerSigning"" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Manager"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Reject"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""280.7692307692307"" Y=""177.9300699300699"" />
     </Transition>
     <Transition Name=""BigBossSigning_Activity_1_1"" To=""AccountingReview "" From=""BigBossSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""BigBoss"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Approve"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""810.7051282051282"" Y=""276.86013986013984"" />
     </Transition>
     <Transition Name=""ManagerSigning_Approved_1"" To=""AccountingReview "" From=""ManagerSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Manager"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Approve"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Otherwise"" />
       </Conditions>
       <Designer X=""456.70512820512835"" Y=""390.69347319347315"" />
     </Transition>
     <Transition Name=""ManagerSigning_BigBossSigning_1"" To=""BigBossSigning"" From=""ManagerSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Manager"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Approve"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Action"" NameRef=""CheckBigBossMustSign"" ConditionInversion=""false"" />
       </Conditions>
       <Designer X=""635.3717948717945"" Y=""225.69347319347304"" />
     </Transition>
     <Transition Name=""Draft_ManagerSigning_1"" To=""ManagerSigning"" From=""VacationRequestCreated"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Author"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""StartSigning"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""278.93589743589735"" Y=""223.09673659673658"" />
     </Transition>
     <Transition Name=""BigBossSigning_ManagerSigning_1"" To=""ManagerSigning"" From=""BigBossSigning"" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""BigBoss"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Reject"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""638.3717948717945"" Y=""179.3601398601398"" />
     </Transition>
     <Transition Name=""ManagerSigning_BigBossSigning_2"" To=""BigBossSigning"" From=""ManagerSigning"" Classifier=""NotSpecified"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Triggers>
         <Trigger Type=""Timer"" NameRef=""SendToBigBoss"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""638.5384615384614"" Y=""136.86013986013987"" />
     </Transition>
     <Transition Name=""Accountant_Activity_1_1"" To=""RequestApproved"" From=""AccountingReview "" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Accountant"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Paid"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""974"" Y=""366"" />
     </Transition>
     <Transition Name=""Accountant_ManagerSigning_1"" To=""ManagerSigning"" From=""AccountingReview "" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" IsFork=""false"" MergeViaSetState=""false"" DisableParentStateControl=""false"">
       <Restrictions>
         <Restriction Type=""Allow"" NameRef=""Accountant"" />
       </Restrictions>
       <Triggers>
         <Trigger Type=""Command"" NameRef=""Reject"" />
       </Triggers>
       <Conditions>
         <Condition Type=""Always"" />
       </Conditions>
       <Designer X=""521.5384615384617"" Y=""340.1934731934732"" />
     </Transition>
   </Transitions>
  <CodeActions>
    <CodeAction Name=""CheckBigBossMustSign"" Type=""Condition"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[var doc = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if(doc == null) return false;
return doc.Sum > 100;]]></ActionCode>
      <Usings><![CDATA[System;System.Collections;System.Collections.Generic;System.Linq;OptimaJet.Workflow;OptimaJet.Workflow.Core.Model;WF.Sample.Business;WF.Sample.Business.Workflow;WF.Sample.Business.DataAccess;]]></Usings>
    </CodeAction>
    <CodeAction Name=""WriteTransitionHistory"" Type=""Action"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[if (processInstance.IdentityIds == null)
    return;

var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.CurrentState);
if(currentstate == null)
{
   currentstate = processInstance.CurrentActivityName;
}
var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
if(nextState == null)
{
    nextState = processInstance.ExecutedActivity.Name;
}
var command = WorkflowInit.Runtime.GetLocalizedCommandName(processInstance.ProcessId, processInstance.CurrentCommand);

var repository = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>();

repository.WriteTransitionHistory(processInstance.ProcessId, currentstate, nextState, command, processInstance.IdentityIds);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections;System.Collections.Generic;System.Linq;OptimaJet.Workflow;OptimaJet.Workflow.Core.Model;WF.Sample.Business;WF.Sample.Business.Workflow;WF.Sample.Business.DataAccess;]]></Usings>
    </CodeAction>
    <CodeAction Name=""UpdateTransitionHistory"" Type=""Action"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[if (string.IsNullOrEmpty(processInstance.CurrentCommand))
    return;
    
var currentstate = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.CurrentState);
if(currentstate == null)
{
   currentstate = processInstance.CurrentActivityName;
}
var nextState = WorkflowInit.Runtime.GetLocalizedStateName(processInstance.ProcessId, processInstance.ExecutedActivityState);
if(nextState == null)
{
    nextState = processInstance.ExecutedActivity.Name;
}
var command = WorkflowInit.Runtime.GetLocalizedCommandName(processInstance.ProcessId, processInstance.CurrentCommand);

if(!string.IsNullOrEmpty(processInstance.ExecutedTimer))
{
    command = string.Format(""Timer: {0}"",processInstance.ExecutedTimer);
}

Guid? employeeId = null;

if (!string.IsNullOrWhiteSpace(processInstance.IdentityId)) employeeId = new Guid(processInstance.IdentityId);

var repository = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>();

repository.UpdateTransitionHistory(processInstance.ProcessId, currentstate, nextState, command, employeeId);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections;System.Collections.Generic;System.Linq;OptimaJet.Workflow;OptimaJet.Workflow.Core.Model;WF.Sample.Business;WF.Sample.Business.Workflow;WF.Sample.Business.DataAccess;]]></Usings>
    </CodeAction>
    <CodeAction Name=""IsDocumentManager"" Type=""RuleCheck"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return false;
return document.ManagerId.HasValue && document.ManagerId.Value == new Guid(identityId);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name=""IsDocumentManager"" Type=""RuleGet"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId, false);

if (document == null || !document.ManagerId.HasValue)
    return new List<string> { };

return new List<string> { document.ManagerId.Value.ToString() };]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name=""IsDocumentAuthor"" Type=""RuleCheck"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return false;
return document.AuthorId == new Guid(identityId);   ]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name=""IsDocumentAuthor"" Type=""RuleGet"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return new List<string> { };
return new List<string> { document.AuthorId.ToString() };]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name=""CheckRole"" Type=""RuleCheck"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[return WorkflowInit.DataServiceProvider.Get<IEmployeeRepository>().CheckRole(new Guid(identityId), parameter);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name=""CheckRole"" Type=""RuleGet"" IsGlobal=""False"" IsAsync=""False"">
      <ActionCode><![CDATA[return WorkflowInit.DataServiceProvider.Get<IEmployeeRepository>().GetInRole(parameter);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
  </CodeActions>
  <Localization>
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""ManagerSigning"" Value=""Manager signing"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""BigBossSigning"" Value=""BigBoss signing"" />
    <Localize Type=""Command"" IsDefault=""True"" Culture=""en-US"" ObjectName=""StartSigning"" Value=""Start signing"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""AccountingReview "" Value=""Accounting review"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""VacationRequestCreated"" Value=""Vacation request created"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""RequestApproved"" Value=""Request approved"" />
  </Localization>
</Process>"
        });

        #endregion
      }
    }
  }
}
