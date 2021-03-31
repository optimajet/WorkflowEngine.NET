/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: FillData.sql
*/

BEGIN TRANSACTION

INSERT dbo.Roles(Id, Name) VALUES ('8d378ebe-0666-46b3-b7ab-1a52480fd12a', N'Big Boss')
INSERT dbo.Roles(Id, Name) VALUES ('412174c2-0490-4101-a7b3-830de90bcaa0', N'Accountant')
INSERT dbo.Roles(Id, Name) VALUES ('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', N'User')

INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('f6e34bdf-b769-42dd-a2be-fee67faf9045', N'Head Group', NULL)
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', N'Group 1', 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('7e9fd972-c775-4c6b-9d91-47e9397bd2e6', N'Group 1.1', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('dc195a4f-46f9-41b2-80d2-77ff9c6269b7', N'Group 1.2', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('72d461b2-234b-40d6-b410-b261964ba291', N'Group 2', 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('c5dcc148-9c0c-45c4-8a68-901d99a26184', N'Group 2.2', '72d461b2-234b-40d6-b410-b261964ba291')
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('bc21a482-28e7-4951-8177-e57813a70fc5', N'Group 2.1', '72d461b2-234b-40d6-b410-b261964ba291')


INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', N'Silviya', 'f6e34bdf-b769-42dd-a2be-fee67faf9045', 1)
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', N'Margo', 'dc195a4f-46f9-41b2-80d2-77ff9c6269b7', 0)
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('deb579f9-991c-4db9-a17d-bb1eccf2842c', N'Max', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b', 1)
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('91f2b471-4a96-4ab7-a41a-ea4293703d16', N'John', '7e9fd972-c775-4c6b-9d91-47e9397bd2e6', 1)
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', N'Maria', 'c5dcc148-9c0c-45c4-8a68-901d99a26184', 0)
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('bbe686f8-8736-48a7-a886-2da25567f978', N'Mark', '7e9fd972-c775-4c6b-9d91-47e9397bd2e6', 0)

INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '412174c2-0490-4101-a7b3-830de90bcaa0')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('bbe686f8-8736-48a7-a886-2da25567f978', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '8d378ebe-0666-46b3-b7ab-1a52480fd12a')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('deb579f9-991c-4db9-a17d-bb1eccf2842c', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('91f2b471-4a96-4ab7-a41a-ea4293703d16', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')



EXEC(N'INSERT dbo.WorkflowScheme(Code, Scheme) VALUES (N''SimpleWF'', N''
<Process Name="SimpleWF">
  <Designer X="-110" Y="-60" />
  <Actors>
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
    <Activity Name="VacationRequestCreated" State="VacationRequestCreated" IsInitial="true" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true" DisablePersist="false">
      <Designer X="20" Y="160" Hidden="false" />
    </Activity>
    <Activity Name="ManagerSigning" State="ManagerSigning" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true" DisablePersist="false">
      <Designer X="360" Y="160" Hidden="false" />
    </Activity>
    <Activity Name="BigBossSigning" State="BigBossSigning" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true" DisablePersist="false">
      <Designer X="640" Y="160" Hidden="false" />
    </Activity>
    <Activity Name="AccountingReview" State="AccountingReview" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true" DisablePersist="false">
      <Designer X="630" Y="320" Hidden="false" />
    </Activity>
    <Activity Name="RequestApproved" State="RequestApproved" IsInitial="false" IsFinal="true" IsForSetState="true" IsAutoSchemeUpdate="true" DisablePersist="false">
      <Designer X="900" Y="320" Hidden="false" />
    </Activity>
  </Activities>
  <Transitions>
    <Transition Name="ManagerSigning_Draft_1" To="VacationRequestCreated" From="ManagerSigning" Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="269" Y="205" Hidden="false" />
    </Transition>
    <Transition Name="BigBossSigning_Activity_1_1" To="AccountingReview" From="BigBossSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Hidden="false" />
    </Transition>
    <Transition Name="ManagerSigning_Approved_1" To="AccountingReview" From="ManagerSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Otherwise" />
      </Conditions>
      <Designer X="453" Y="271" Hidden="false" />
    </Transition>
    <Transition Name="ManagerSigning_BigBossSigning_1" To="BigBossSigning" From="ManagerSigning" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Manager" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Approve" />
      </Triggers>
      <Conditions>
        <Condition Type="Expression" ConditionInversion="false">
          <Expression><![CDATA[@Sum > 100]]></Expression>
        </Condition>
      </Conditions>
      <Designer X="562" Y="177" Hidden="false" />
    </Transition>
    <Transition Name="Draft_ManagerSigning_1" To="ManagerSigning" From="VacationRequestCreated" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="StartSigning" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="269" Y="170" Hidden="false" />
    </Transition>
    <Transition Name="BigBossSigning_ManagerSigning_1" To="ManagerSigning" From="BigBossSigning" Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="563" Y="204" Hidden="false" />
    </Transition>
    <Transition Name="ManagerSigning_BigBossSigning_2" To="BigBossSigning" From="ManagerSigning" Classifier="NotSpecified" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Triggers>
        <Trigger Type="Timer" NameRef="SendToBigBoss" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="557" Y="120" Hidden="false" />
    </Transition>
    <Transition Name="Accountant_Activity_1_1" To="RequestApproved" From="AccountingReview" Classifier="Direct" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Paid" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Hidden="false" />
    </Transition>
    <Transition Name="Accountant_ManagerSigning_1" To="ManagerSigning" From="AccountingReview" Classifier="Reverse" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Reject" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="395" Y="343" Hidden="false" />
    </Transition>
  </Transitions>
  <Localization>
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="ManagerSigning" Value="Manager signing" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="BigBossSigning" Value="BigBoss signing" />
    <Localize Type="Command" IsDefault="True" Culture="en-US" ObjectName="StartSigning" Value="Start signing" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="AccountingReview" Value="Accounting review" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="VacationRequestCreated" Value="Vacation request created" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="RequestApproved" Value="Request approved" />
  </Localization>
</Process>
'')')


COMMIT TRANSACTION