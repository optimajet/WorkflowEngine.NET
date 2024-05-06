IF NOT EXISTS(SELECT * FROM dbo.Roles WHERE Id = '8d378ebe-0666-46b3-b7ab-1a52480fd12a' AND Name = N'Big Boss')
BEGIN
INSERT dbo.Roles(Id, Name) VALUES ('8d378ebe-0666-46b3-b7ab-1a52480fd12a', N'Big Boss')
END
    IF NOT EXISTS(SELECT * FROM dbo.Roles WHERE Id = '412174c2-0490-4101-a7b3-830de90bcaa0' AND Name = N'Accountant')
BEGIN
INSERT dbo.Roles(Id, Name) VALUES ('412174c2-0490-4101-a7b3-830de90bcaa0', N'Accountant')
END
    IF NOT EXISTS(SELECT * FROM dbo.Roles WHERE Id = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb' AND Name = N'User')
BEGIN
INSERT dbo.Roles(Id, Name) VALUES ('71fffb5b-b707-4b3c-951c-c37fdfcc8dfb', N'User')
END

    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = 'f6e34bdf-b769-42dd-a2be-fee67faf9045' AND Name = N'Head Group' AND ParentId IS NULL)
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('f6e34bdf-b769-42dd-a2be-fee67faf9045', N'Head Group', NULL)
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b' AND Name = N'Group 1' AND ParentId = 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('b14f5d81-5b0d-4acc-92b8-27cbbe39086b', N'Group 1', 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = '7e9fd972-c775-4c6b-9d91-47e9397bd2e6' AND Name = N'Group 1.1' AND ParentId = 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('7e9fd972-c775-4c6b-9d91-47e9397bd2e6', N'Group 1.1', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = 'dc195a4f-46f9-41b2-80d2-77ff9c6269b7' AND Name = N'Group 1.2' AND ParentId = 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('dc195a4f-46f9-41b2-80d2-77ff9c6269b7', N'Group 1.2', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b')
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = '72d461b2-234b-40d6-b410-b261964ba291' AND Name = N'Group 2' AND ParentId = 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('72d461b2-234b-40d6-b410-b261964ba291', N'Group 2', 'f6e34bdf-b769-42dd-a2be-fee67faf9045')
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = 'c5dcc148-9c0c-45c4-8a68-901d99a26184' AND Name = N'Group 2.2' AND ParentId = '72d461b2-234b-40d6-b410-b261964ba291')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('c5dcc148-9c0c-45c4-8a68-901d99a26184', N'Group 2.2', '72d461b2-234b-40d6-b410-b261964ba291')
END
    IF NOT EXISTS(SELECT * FROM dbo.StructDivision WHERE Id = 'bc21a482-28e7-4951-8177-e57813a70fc5' AND Name = N'Group 2.1' AND ParentId = '72d461b2-234b-40d6-b410-b261964ba291')
BEGIN
INSERT dbo.StructDivision(Id, Name, ParentId) VALUES ('bc21a482-28e7-4951-8177-e57813a70fc5', N'Group 2.1', '72d461b2-234b-40d6-b410-b261964ba291')
END

    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = '81537e21-91c5-4811-a546-2dddff6bf409' AND Name = N'Silviya' AND StructDivisionId = 'f6e34bdf-b769-42dd-a2be-fee67faf9045' AND IsHead = 1)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', N'Silviya', 'f6e34bdf-b769-42dd-a2be-fee67faf9045', 1)
END
    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = 'b0e6fd4c-2db9-4bb6-a62e-68b6b8999905' AND Name = N'Margo' AND StructDivisionId = 'dc195a4f-46f9-41b2-80d2-77ff9c6269b7' AND IsHead = 0)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', N'Margo', 'dc195a4f-46f9-41b2-80d2-77ff9c6269b7', 0)
END
    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = 'deb579f9-991c-4db9-a17d-bb1eccf2842c' AND Name = N'Max' AND StructDivisionId = 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b' AND IsHead = 1)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('deb579f9-991c-4db9-a17d-bb1eccf2842c', N'Max', 'b14f5d81-5b0d-4acc-92b8-27cbbe39086b', 1)
END
    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = '91f2b471-4a96-4ab7-a41a-ea4293703d16' AND Name = N'John' AND StructDivisionId = '7e9fd972-c775-4c6b-9d91-47e9397bd2e6' AND IsHead = 1)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('91f2b471-4a96-4ab7-a41a-ea4293703d16', N'John', '7e9fd972-c775-4c6b-9d91-47e9397bd2e6', 1)
END
    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = 'e41b48e3-c03d-484f-8764-1711248c4f8a' AND Name = N'Maria' AND StructDivisionId = 'c5dcc148-9c0c-45c4-8a68-901d99a26184' AND IsHead = 0)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', N'Maria', 'c5dcc148-9c0c-45c4-8a68-901d99a26184', 0)
END
    IF NOT EXISTS(SELECT * FROM dbo.Employee WHERE Id = 'bbe686f8-8736-48a7-a886-2da25567f978' AND Name = N'Mark' AND StructDivisionId = '7e9fd972-c775-4c6b-9d91-47e9397bd2e6' AND IsHead = 0)
BEGIN
INSERT dbo.Employee(Id, Name, StructDivisionId, IsHead) VALUES ('bbe686f8-8736-48a7-a886-2da25567f978', N'Mark', '7e9fd972-c775-4c6b-9d91-47e9397bd2e6', 0)
END

    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = 'e41b48e3-c03d-484f-8764-1711248c4f8a' AND RoleId = '412174c2-0490-4101-a7b3-830de90bcaa0')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '412174c2-0490-4101-a7b3-830de90bcaa0')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = 'e41b48e3-c03d-484f-8764-1711248c4f8a' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('e41b48e3-c03d-484f-8764-1711248c4f8a', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = 'bbe686f8-8736-48a7-a886-2da25567f978' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('bbe686f8-8736-48a7-a886-2da25567f978', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = '81537e21-91c5-4811-a546-2dddff6bf409' AND RoleId = '8d378ebe-0666-46b3-b7ab-1a52480fd12a')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '8d378ebe-0666-46b3-b7ab-1a52480fd12a')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = '81537e21-91c5-4811-a546-2dddff6bf409' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('81537e21-91c5-4811-a546-2dddff6bf409', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = 'b0e6fd4c-2db9-4bb6-a62e-68b6b8999905' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('b0e6fd4c-2db9-4bb6-a62e-68b6b8999905', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = 'deb579f9-991c-4db9-a17d-bb1eccf2842c' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('deb579f9-991c-4db9-a17d-bb1eccf2842c', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END
    IF NOT EXISTS(SELECT * FROM dbo.EmployeeRole WHERE EmployeeId = '91f2b471-4a96-4ab7-a41a-ea4293703d16' AND RoleId = '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
BEGIN
INSERT dbo.EmployeeRole(EmployeeId, RoleId) VALUES ('91f2b471-4a96-4ab7-a41a-ea4293703d16', '71fffb5b-b707-4b3c-951c-c37fdfcc8dfb')
END


    IF NOT EXISTS(SELECT * FROM dbo.WorkflowScheme WHERE Code = N'SimpleWF')
BEGIN
EXEC(N'INSERT dbo.WorkflowScheme(Code, Scheme) VALUES (N''SimpleWF'', N''
<Process Name="SimpleWF" CanBeInlined="false" Tags="" LogEnabled="false">
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
  <Comments>
    <Comment Name="Comment" Alignment="left" Rotation="0" Width="168" BoldText="false" ItalicText="false" UnderlineText="false" LineThroughText="false" FontSize="14" Value="↑&#xA;&#xA;This is the Activity and this is the first key object which makes up the diagram.&#xA;It specifies the order in which Actions are performed in your process.">
      <Designer X="330" Y="290" Color="#020930" Hidden="false" />
    </Comment>
    <Comment Name="Comment" Alignment="left" Rotation="0" Width="253" BoldText="false" ItalicText="false" UnderlineText="false" LineThroughText="false" FontSize="14" Value="← This Transition is triggered by a timer.">
      <Designer X="1020" Y="120" Color="#020930" Hidden="false" />
    </Comment>
    <Comment Name="Comment" Alignment="center" Rotation="0" Width="157" BoldText="false" ItalicText="false" UnderlineText="false" LineThroughText="false" FontSize="14" Value=" This Transition is &#xA;triggered by a command &#xA;with condition&#xA;↓">
      <Designer X="820" Y="160" Color="#020930" Hidden="false" />
    </Comment>
    <Comment Name="Comment" Alignment="right" Rotation="0" Width="271" BoldText="false" ItalicText="false" UnderlineText="false" LineThroughText="false" FontSize="14" Value="This is the Transition and this is the second key object a scheme comprises.&#xA;Transition always connects two Activities together, and controls the sequence of execution of your processes.&#xA;&#xA;↓">
      <Designer X="330" Y="110" Color="#020930" Hidden="false" />
    </Comment>
  </Comments>
  <Activities>
    <Activity Name="VacationRequestCreated" State="VacationRequestCreated" IsInitial="true" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true">
      <Designer X="320" Y="220" Color="#27AE60" Hidden="false" />
    </Activity>
    <Activity Name="ManagerSigning" State="ManagerSigning" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true">
      <Designer X="660" Y="220" Hidden="false" />
    </Activity>
    <Activity Name="BigBossSigning" State="BigBossSigning" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true">
      <Designer X="1000" Y="220" Hidden="false" />
    </Activity>
    <Activity Name="AccountingReview" State="AccountingReview" IsInitial="false" IsFinal="false" IsForSetState="true" IsAutoSchemeUpdate="true">
      <Designer X="1000" Y="380" Hidden="false" />
    </Activity>
    <Activity Name="RequestApproved" State="RequestApproved" IsInitial="false" IsFinal="true" IsForSetState="true" IsAutoSchemeUpdate="true">
      <Designer X="1280" Y="380" Hidden="false" />
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
      <Designer X="575" Y="236" Hidden="false" />
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
      <Designer X="761" Y="334" Hidden="false" />
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
      <Designer X="905" Y="238" Hidden="false" />
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
      <Designer X="572" Y="268" Hidden="false" />
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
      <Designer X="902" Y="268" Hidden="false" />
    </Transition>
    <Transition Name="ManagerSigning_BigBossSigning_2" To="BigBossSigning" From="ManagerSigning" Classifier="NotSpecified" AllowConcatenationType="And" RestrictConcatenationType="And" ConditionsConcatenationType="And" DisableParentStateControl="false">
      <Triggers>
        <Trigger Type="Timer" NameRef="SendToBigBoss" />
      </Triggers>
      <Conditions>
        <Condition Type="Always" />
      </Conditions>
      <Designer X="903" Y="122" Hidden="false" />
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
      <Designer X="702" Y="420" Hidden="false" />
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
END