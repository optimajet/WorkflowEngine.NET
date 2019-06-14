/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: FillData.sql
*/
START TRANSACTION;

INSERT INTO `Roles`(`Id`, `Name`) VALUES (UUID_TO_BIN('8d378ebe-0666-46b3-b7ab-1a52480fd12a', false), 'Big Boss');
INSERT INTO `Roles`(`Id`, `Name`) VALUES (UUID_TO_BIN('412174c2-0490-4101-a7b3-830de90bcaa0', false), 'Accountant');
INSERT INTO `Roles`(`Id`, `Name`) VALUES (UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false), 'User');

INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('f6e34bdf-b769-42dd-a2be-fee67faf9045', false), 'Head Group', NULL);
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', false), 'Group 1', UUID_TO_BIN('f6e34bdf-b769-42dd-a2be-fee67faf9045', false));
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('7e9fd972-c775-4c6b-9d91-47e9397bd2e6', false), 'Group 1.1', UUID_TO_BIN('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', false));
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('dc195a4f-46f9-41b2-80d2-77ff9c6269b7', false), 'Group 1.2', UUID_TO_BIN('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', false));
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('72d461b2-234b-40d6-b410-b261964ba291', false), 'Group 2', UUID_TO_BIN('f6e34bdf-b769-42dd-a2be-fee67faf9045', false));
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('c5dcc148-9c0c-45c4-8a68-901d99a26184', false), 'Group 2.2', UUID_TO_BIN('72d461b2-234b-40d6-b410-b261964ba291', false));
INSERT INTO `StructDivision`(`Id`, `Name`, `ParentId`) VALUES (UUID_TO_BIN('bc21a482-28e7-4951-8177-e57813a70fc5', false), 'Group 2.1', UUID_TO_BIN('72d461b2-234b-40d6-b410-b261964ba291', false));


INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('81537e21-91c5-4811-a546-2dddff6bf409', false), 'Silviya', UUID_TO_BIN('f6e34bdf-b769-42dd-a2be-fee67faf9045', false), 1);
INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', false), 'Margo', UUID_TO_BIN('dc195a4f-46f9-41b2-80d2-77ff9c6269b7', false), 0);
INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('deb579f9-991c-4db9-a17d-bb1eccf2842c', false), 'Max', UUID_TO_BIN('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', false), 1);
INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('91f2b471-4a96-4ab7-a41a-ea4293703d16', false), 'John', UUID_TO_BIN('7e9fd972-c775-4c6b-9d91-47e9397bd2e6', false), 1);
INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('e41b48e3-c03d-484f-8764-1711248c4f8a', false), 'Maria', UUID_TO_BIN('c5dcc148-9c0c-45c4-8a68-901d99a26184', false), 0);
INSERT INTO `Employee`(`Id`, `Name`, `StructDivisionId`, `IsHead`) VALUES (UUID_TO_BIN('bbe686f8-8736-48a7-a886-2da25567f978', false), 'Mark', UUID_TO_BIN('7e9fd972-c775-4c6b-9d91-47e9397bd2e6', false), 0);

INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('e41b48e3-c03d-484f-8764-1711248c4f8a', false), UUID_TO_BIN('412174c2-0490-4101-a7b3-830de90bcaa0', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('e41b48e3-c03d-484f-8764-1711248c4f8a', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('bbe686f8-8736-48a7-a886-2da25567f978', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('81537e21-91c5-4811-a546-2dddff6bf409', false), UUID_TO_BIN('8d378ebe-0666-46b3-b7ab-1a52480fd12a', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('81537e21-91c5-4811-a546-2dddff6bf409', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('deb579f9-991c-4db9-a17d-bb1eccf2842c', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));
INSERT INTO `EmployeeRole`(`EmployeeId`, `RoleId`) VALUES (UUID_TO_BIN('91f2b471-4a96-4ab7-a41a-ea4293703d16', false), UUID_TO_BIN('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', false));

INSERT INTO `WorkflowScheme`(`Code`, `Scheme`) VALUES ('SimpleWF', '
<Process Name="SimpleWF">
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
</Process>'
);

COMMIT;