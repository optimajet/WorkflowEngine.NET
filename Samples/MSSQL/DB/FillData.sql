/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET 1.3 For Azure
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

INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '412174c2-0490-4101-a7b3-830de90bcaa0')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('bbe686f8-8736-48a7-a886-2da25567f978', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '8d378ebe-0666-46b3-b7ab-1a52480fd12a')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('deb579f9-991c-4db9-a17d-bb1eccf2842c', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
INSERT dbo.EmployeeRole(EmloyeeId, RoleId) VALUES ('91f2b471-4a96-4ab7-a41a-ea4293703d16', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')


EXEC(N'INSERT dbo.WorkflowScheme(Code, Scheme) VALUES (N''SimpleWF'', N''<Process Name="SimpleWF">
  <Designer X="-110" Y="-60" />
  <Actors>
    <Actor Name="Author" Rule="IsDocumentAuthor" Value="" />
    <Actor Name="AuthorsBoss" Rule="IsAuthorsBoss" Value="" />
    <Actor Name="Controller" Rule="IsDocumentController" Value="" />
    <Actor Name="BigBoss" Rule="CheckRole" Value="Big Boss" />
    <Actor Name="Accountant" Rule="CheckRole" Value="Accountant" />
  </Actors>
  <Parameters>
    <Parameter Name="Comment" Type="System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" Purpose="Temporary" />
  </Parameters>
  <Commands>
    <Command Name="StartProcessing">
      <InputParameters>
        <ParameterRef Name="Comment" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Sighting">
      <InputParameters>
        <ParameterRef Name="Comment" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Denial">
      <InputParameters>
        <ParameterRef Name="Comment" NameRef="Comment" />
      </InputParameters>
    </Command>
    <Command Name="Paid">
      <InputParameters>
        <ParameterRef Name="Comment" NameRef="Comment" />
      </InputParameters>
    </Command>
  </Commands>
  <Timers>
    <Timer Name="ControllerTimer" Type="Interval" Value="120000" NotOverrideIfExists="false" />
  </Timers>
  <Activities>
    <Activity Name="DraftInitial" State="Draft" IsInitial="True" IsFinal="False" IsForSetState="False" IsAutoSchemeUpdate="False">
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="100" Y="90" />
    </Activity>
    <Activity Name="Draft" State="Draft" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="120" Y="270" />
    </Activity>
    <Activity Name="DraftStartProcessingExecute" IsInitial="False" IsFinal="False" IsForSetState="False" IsAutoSchemeUpdate="False">
      <Designer X="400" Y="90" />
    </Activity>
    <Activity Name="ControllerSighting" State="ControllerSighting" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="400" Y="270" />
    </Activity>
    <Activity Name="ControllerSightingExecute" IsInitial="False" IsFinal="False" IsForSetState="False" IsAutoSchemeUpdate="False">
      <Designer X="650" Y="130" />
    </Activity>
    <Activity Name="AuthorBossSighting" State="AuthorBossSighting" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="650" Y="320" />
    </Activity>
    <Activity Name="AuthorConfirmation" State="AuthorConfirmation" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="970" Y="210" />
    </Activity>
    <Activity Name="BigBossSighting" State="BigBossSighting" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="970" Y="410" />
    </Activity>
    <Activity Name="AccountantProcessing" State="AccountantProcessing" IsInitial="False" IsFinal="False" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="1240" Y="300" />
    </Activity>
    <Activity Name="Paid" State="Paid" IsInitial="False" IsFinal="True" IsForSetState="True" IsAutoSchemeUpdate="True">
      <Implementation>
        <ActionRef Order="1" NameRef="UpdateTransitionHistory" />
      </Implementation>
      <PreExecutionImplementation>
        <ActionRef Order="1" NameRef="WriteTransitionHistory" />
      </PreExecutionImplementation>
      <Designer X="1240" Y="480" />
    </Activity>
  </Activities>
  <Transitions>
    <Transition Name="DraftInitial" To="DraftStartProcessingExecute" From="DraftInitial" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="StartProcessing" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0" />
    </Transition>
    <Transition Name="Draft" To="ControllerSighting" From="Draft" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="StartProcessing" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0" />
    </Transition>
    <Transition Name="DraftStartProcessingExecute_1" To="ControllerSighting" From="DraftStartProcessingExecute" Classifier="Direct">
      <Triggers>
        <Trigger Type="Auto" />
      </Triggers>
      <Conditions>
        <Condition Type="Action" NameRef="CheckDocumentHasController" />
      </Conditions>
      <Designer Bending="" />
    </Transition>
    <Transition Name="DraftStartProcessingExecute_2" To="ControllerSightingExecute" From="DraftStartProcessingExecute" Classifier="Direct">
      <Triggers>
        <Trigger Type="Auto" />
      </Triggers>
      <Conditions>
        <Condition Type="Otherwise" />
      </Conditions>
      <Designer Bending="0" />
    </Transition>
    <Transition Name="ControllerSighting" To="ControllerSightingExecute" From="ControllerSighting" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Controller" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Sighting" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0.18861156996410458" />
    </Transition>
    <Transition Name="ControllerSighting_R" To="Draft" From="ControllerSighting" Classifier="Reverse">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Controller" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Denial" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="" />
    </Transition>
    <Transition Name="ControllerSightingExecute_1" To="AuthorConfirmation" From="ControllerSightingExecute" Classifier="Direct">
      <Triggers>
        <Trigger Type="Auto" />
      </Triggers>
      <Conditions>
        <Condition Type="Action" NameRef="CheckDocumentsAuthorIsBoss" />
      </Conditions>
      <Designer Bending="0.13091527870458927" />
    </Transition>
    <Transition Name="ControllerSightingExecute_2" To="AuthorBossSighting" From="ControllerSightingExecute" Classifier="Direct">
      <Triggers>
        <Trigger Type="Auto" />
      </Triggers>
      <Conditions>
        <Condition Type="Otherwise" />
      </Conditions>
      <Designer Bending="" />
    </Transition>
    <Transition Name="AuthorBossSighting" To="AuthorConfirmation" From="AuthorBossSighting" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="AuthorsBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Sighting" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0" />
    </Transition>
    <Transition Name="AuthorBossSighting_R" To="Draft" From="AuthorBossSighting" Classifier="Reverse">
      <Restrictions>
        <Restriction Type="Allow" NameRef="AuthorsBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Denial" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="-0.10762234343917368" />
    </Transition>
    <Transition Name="AuthorConfirmation_1" To="BigBossSighting" From="AuthorConfirmation" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Sighting" />
      </Triggers>
      <Conditions>
        <Condition Type="Action" NameRef="CheckBigBossMustSight" />
      </Conditions>
      <Designer Bending="" />
    </Transition>
    <Transition Name="AuthorConfirmation_2" To="AccountantProcessing" From="AuthorConfirmation" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Sighting" />
      </Triggers>
      <Conditions>
        <Condition Type="Otherwise" />
      </Conditions>
      <Designer Bending="-0.3508719297850871" />
    </Transition>
    <Transition Name="AuthorConfirmation_R" To="Draft" From="AuthorConfirmation" Classifier="Reverse">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Author" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Denial" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="-0.2916249379586396" />
    </Transition>
    <Transition Name="BigBossSighting" To="AccountantProcessing" From="BigBossSighting" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Sighting" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0" />
    </Transition>
    <Transition Name="BigBossSighting_R" To="Draft" From="BigBossSighting" Classifier="Reverse">
      <Restrictions>
        <Restriction Type="Allow" NameRef="BigBoss" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Denial" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="-0.20253979975959344" />
    </Transition>
    <Transition Name="AccountantProcessing" To="Paid" From="AccountantProcessing" Classifier="Direct">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Paid" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="" />
    </Transition>
    <Transition Name="AccountantProcessing_R" To="AuthorConfirmation" From="AccountantProcessing" Classifier="Reverse">
      <Restrictions>
        <Restriction Type="Allow" NameRef="Accountant" />
      </Restrictions>
      <Triggers>
        <Trigger Type="Command" NameRef="Denial" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="0.19834025131435484" />
    </Transition>
    <Transition Name="ControllerSighting_ControllerSightingExecute_1" To="ControllerSightingExecute" From="ControllerSighting" Classifier="NotSpecified">
      <Triggers>
        <Trigger Type="Timer" NameRef="ControllerTimer" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer Bending="-0.14296569235026" />
    </Transition>
  </Transitions>
  <Localization>
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="ControllerSighting" Value="Controller sighting" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="AuthorBossSighting" Value="Author''''s boss sighting" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="AuthorConfirmation" Value="Author confirmation" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="BigBossSighting" Value="BigBoss sighting" />
    <Localize Type="State" IsDefault="True" Culture="en-US" ObjectName="AccountantProcessing" Value="Accountant processing" />
    <Localize Type="Command" IsDefault="True" Culture="en-US" ObjectName="StartProcessing" Value="Start processing" />
  </Localization>
</Process>'')')


COMMIT TRANSACTION