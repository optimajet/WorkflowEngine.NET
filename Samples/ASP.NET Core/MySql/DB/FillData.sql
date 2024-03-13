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

INSERT INTO `workflowscheme`(`Code`, `Scheme`) VALUES ('SimpleWF', '
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
');

COMMIT;