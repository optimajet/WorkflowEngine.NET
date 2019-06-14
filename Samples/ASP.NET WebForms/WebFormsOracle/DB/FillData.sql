/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: FillData.sql
*/

INSERT INTO Roles(Id, Name) VALUES ('8D378EBE066646B3B7AB1A52480FD12A', 'Big Boss');
INSERT INTO Roles(Id, Name) VALUES ('412174C204904101A7B3830DE90BCAA0', 'Accountant');
INSERT INTO Roles(Id, Name) VALUES ('71FFFB5BB7074B3C951CC37FDFCC8DFB', 'User');

INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('F6E34BDFB76942DDA2BEFEE67FAF9045', 'Head Group', NULL);
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('B14F5D815B0D4ACC92B827CBBE39086B', 'Group 1', 'F6E34BDFB76942DDA2BEFEE67FAF9045');
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('7E9FD972C7754C6B9D9147E9397BD2E6', 'Group 1.1', 'B14F5D815B0D4ACC92B827CBBE39086B');
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('DC195A4F46F941B280D277FF9C6269B7', 'Group 1.2', 'B14F5D815B0D4ACC92B827CBBE39086B');
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('72D461B2234B40D6B410B261964BA291', 'Group 2', 'F6E34BDFB76942DDA2BEFEE67FAF9045');
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('C5DCC1489C0C45C48A68901D99A26184', 'Group 2.2', '72D461B2234B40D6B410B261964BA291');
INSERT INTO StructDivision(Id, Name, ParentId) VALUES ('BC21A48228E749518177E57813A70FC5', 'Group 2.1', '72D461B2234B40D6B410B261964BA291');


INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('81537E2191C54811A5462DDDFF6BF409', 'Silviya', 'F6E34BDFB76942DDA2BEFEE67FAF9045', 1);
INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('B0E6FD4C2DB94BB6A62E68B6B8999905', 'Margo', 'DC195A4F46F941B280D277FF9C6269B7', 0);
INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('DEB579F9991C4DB9A17DBB1ECCF2842C', 'Max', 'B14F5D815B0D4ACC92B827CBBE39086B', 1);
INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('91F2B4714A964AB7A41AEA4293703D16', 'John', '7E9FD972C7754C6B9D9147E9397BD2E6', 1);
INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('E41B48E3C03D484F87641711248C4F8A', 'Maria', 'C5DCC1489C0C45C48A68901D99A26184', 0);
INSERT INTO Employee(Id, Name, StructDivisionId, IsHead) VALUES ('BBE686F8873648A7A8862DA25567F978', 'Mark', '7E9FD972C7754C6B9D9147E9397BD2E6', 0);

INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('E41B48E3C03D484F87641711248C4F8A', '412174C204904101A7B3830DE90BCAA0');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('E41B48E3C03D484F87641711248C4F8A', '71FFFB5BB7074B3C951CC37FDFCC8DFB');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('BBE686F8873648A7A8862DA25567F978', '71FFFB5BB7074B3C951CC37FDFCC8DFB');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('81537E2191C54811A5462DDDFF6BF409', '8D378EBE066646B3B7AB1A52480FD12A');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('81537E2191C54811A5462DDDFF6BF409', '71FFFB5BB7074B3C951CC37FDFCC8DFB');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('B0E6FD4C2DB94BB6A62E68B6B8999905', '71FFFB5BB7074B3C951CC37FDFCC8DFB');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('DEB579F9991C4DB9A17DBB1ECCF2842C', '71FFFB5BB7074B3C951CC37FDFCC8DFB');
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID) VALUES ('91F2B4714A964AB7A41AEA4293703D16', '71FFFB5BB7074B3C951CC37FDFCC8DFB');


DECLARE
  schemeValue varchar2(32767);
BEGIN
  schemeValue := '<Process Name="SimpleWF">
  <Designer X="-110" Y="-60" />
  <Actors>
    <Actor Name="Author" Rule="IsDocumentAuthor" Value="" />
    <Actor Name="Manager" Rule="IsDocumentManager" Value="" />
    <Actor Name="BigBoss" Rule="CheckRole" Value="Big Boss" />
    <Actor Name="Accountant" Rule="CheckRole" Value="Accountant" />
  </Actors>
  <Parameters>
    <Parameter Name="Comment" Type="String" Purpose="Temporary" />
  </Parameters>
  <Commands>
    <Command Name="StartSigning">
      <InputParameters>
        <ParameterRef Name="Comment" IsRequired="false" DefaultValue="" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Approve">
      <InputParameters>
        <ParameterRef Name="Comment" IsRequired="false" DefaultValue="" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Reject">
      <InputParameters>
        <ParameterRef Name="Comment" IsRequired="false" DefaultValue="" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Paid">
      <InputParameters>
        <ParameterRef Name="Comment" IsRequired="false" DefaultValue="" NameRef="Comment" />
      </InputParameters>
    </Command>
  </Commands>
  <Timers>
    <Timer Name="SendToBigBoss" Type="Interval" Value="10minutes" NotOverrideIfExists="false" />
  </Timers>
  <Activities>
    <Activity Name="VacationRequestCreated" State="VacationRequestCreated" IsInitial="True" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="10" Y="170" />
    </Activity>
    <Activity Name="ManagerSigning" State="ManagerSigning" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="361.53846153846166" Y="172.69347319347324" />
    </Activity>
    <Activity Name="BigBossSigning" State="BigBossSigning" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="721.5384615384614" Y="172.6934731934732" />
    </Activity>
    <Activity Name="AccountingReview " State="AccountingReview " IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="718.2051282051282" Y="334.3601398601398" />
    </Activity>
    <Activity Name="RequestApproved" State="RequestApproved" IsInitial="False" IsFinal="True" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="1036.5384615384614" Y="334.3601398601398" />
    </Activity>
  </Activities>
  <Transitions>
    <Transition Name="ManagerSigning_Draft_1" To="VacationRequestCreated" From="ManagerSigning" Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="280.7692307692307" Y="177.9300699300699" />
    </Transition>
    <Transition Name="BigBossSigning_Activity_1_1" To="AccountingReview " From="BigBossSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="810.7051282051282" Y="276.86013986013984" />
    </Transition>
    <Transition Name="ManagerSigning_Approved_1" To="AccountingReview " From="ManagerSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Otherwise" />
      </Conditions>
      <Designer X="456.70512820512835" Y="390.69347319347315" />
    </Transition>
    <Transition Name="ManagerSigning_BigBossSigning_1" To="BigBossSigning" From="ManagerSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Action" NameRef="CheckBigBossMustSign" ConditionInversion="false" />
      </Conditions>
      <Designer X="635.3717948717945" Y="225.69347319347304" />
    </Transition>
    <Transition Name="Draft_ManagerSigning_1" To="ManagerSigning" From="VacationRequestCreated" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="StartSigning" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="278.93589743589735" Y="223.09673659673658" />
    </Transition>
    <Transition Name="BigBossSigning_ManagerSigning_1" To="ManagerSigning" From="BigBossSigning" Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="638.3717948717945" Y="179.3601398601398" />
    </Transition>
    <Transition Name="ManagerSigning_BigBossSigning_2" To="BigBossSigning" From="ManagerSigning" Classifier="NotSpecified" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Triggers>
        <Trigger Type="Timer" NameRef="SendToBigBoss" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="638.5384615384614" Y="136.86013986013987" />
    </Transition>
    <Transition Name="Accountant_Activity_1_1" To="RequestApproved" From="AccountingReview " Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Paid" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="974" Y="366" />
    </Transition>
    <Transition Name="Accountant_ManagerSigning_1" To="ManagerSigning" From="AccountingReview " Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" IsFork="false" MergeViaSetState="false" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="521.5384615384617" Y="340.1934731934732" />
    </Transition>
  </Transitions>
  <CodeActions>
    <CodeAction Name="CheckBigBossMustSign" Type="Condition" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[var doc = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if(doc == null) return false;
return doc.Sum > 100;]]></ActionCode>
      <Usings><![CDATA[System;System.Collections;System.Collections.Generic;System.Linq;OptimaJet.Workflow;OptimaJet.Workflow.Core.Model;WF.Sample.Business;WF.Sample.Business.Workflow;WF.Sample.Business.DataAccess;]]></Usings>
    </CodeAction>
    <CodeAction Name="WriteTransitionHistory" Type="Action" IsGlobal="False" IsAsync="False">
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
    <CodeAction Name="UpdateTransitionHistory" Type="Action" IsGlobal="False" IsAsync="False">
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
    command = string.Format("Timer: {0}",processInstance.ExecutedTimer);
}

Guid? employeeId = null;

if (!string.IsNullOrWhiteSpace(processInstance.IdentityId)) employeeId = new Guid(processInstance.IdentityId);

var repository = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>();

repository.UpdateTransitionHistory(processInstance.ProcessId, currentstate, nextState, command, employeeId);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections;System.Collections.Generic;System.Linq;OptimaJet.Workflow;OptimaJet.Workflow.Core.Model;WF.Sample.Business;WF.Sample.Business.Workflow;WF.Sample.Business.DataAccess;]]></Usings>
    </CodeAction>
    <CodeAction Name="IsDocumentManager" Type="RuleCheck" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return false;
return document.ManagerId.HasValue && document.ManagerId.Value == new Guid(identityId);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name="IsDocumentManager" Type="RuleGet" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId, false);

if (document == null || !document.ManagerId.HasValue)
    return new List<string> { };

return new List<string> { document.ManagerId.Value.ToString() };]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name="IsDocumentAuthor" Type="RuleCheck" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return false;
return document.AuthorId == new Guid(identityId);   ]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name="IsDocumentAuthor" Type="RuleGet" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[var document = WorkflowInit.DataServiceProvider.Get<IDocumentRepository>().Get(processInstance.ProcessId);
if (document == null)
    return new List<string> { };
return new List<string> { document.AuthorId.ToString() };]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name="CheckRole" Type="RuleCheck" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[return WorkflowInit.DataServiceProvider.Get<IEmployeeRepository>().CheckRole(new Guid(identityId), parameter);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
    <CodeAction Name="CheckRole" Type="RuleGet" IsGlobal="False" IsAsync="False">
      <ActionCode><![CDATA[return WorkflowInit.DataServiceProvider.Get<IEmployeeRepository>().GetInRole(parameter);]]></ActionCode>
      <Usings><![CDATA[System;System.Collections.Generic;System.Linq;OptimaJet.Workflow.Core.Runtime;OptimaJet.Workflow.Core.Model;WF.Sample.Business.DataAccess;WF.Sample.Business.Workflow;]]></Usings>
    </CodeAction>
  </CodeActions>
  <Localization>
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="ManagerSigning" Value="Manager signing" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="BigBossSigning" Value="BigBoss signing" />
    <Localize Type="Command" IsDefault="True" Culture="en-US" ObjectName="StartSigning" Value="Start signing" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="AccountingReview " Value="Accounting review" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="VacationRequestCreated" Value="Vacation request created" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="RequestApproved" Value="Request approved" />
  </Localization>
</Process>';
  INSERT INTO WorkflowScheme(Code, Scheme) VALUES ('SimpleWF', schemeValue);
END;
/

COMMIT;