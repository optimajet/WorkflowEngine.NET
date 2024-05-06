BEGIN
INSERT INTO Roles(Id, Name) 
SELECT '8D378EBE066646B3B7AB1A52480FD12A', 'Big Boss'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Id = '8D378EBE066646B3B7AB1A52480FD12A' AND Name = 'Big Boss'
);
INSERT INTO Roles(Id, Name)
SELECT '412174C204904101A7B3830DE90BCAA0', 'Accountant'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Id = '412174C204904101A7B3830DE90BCAA0' AND Name = 'Accountant'
);
INSERT INTO Roles(Id, Name)
SELECT '71FFFB5BB7074B3C951CC37FDFCC8DFB', 'User'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM Roles WHERE Id = '71FFFB5BB7074B3C951CC37FDFCC8DFB' AND Name = 'User'
);

INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT 'F6E34BDFB76942DDA2BEFEE67FAF9045', 'Head Group', NULL
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = 'F6E34BDFB76942DDA2BEFEE67FAF9045' AND Name = 'Head Group' AND ParentId IS NULL
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT 'B14F5D815B0D4ACC92B827CBBE39086B', 'Group 1', 'F6E34BDFB76942DDA2BEFEE67FAF9045'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = 'B14F5D815B0D4ACC92B827CBBE39086B' AND Name = 'Group 1' AND ParentId = 'F6E34BDFB76942DDA2BEFEE67FAF9045'
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT '7E9FD972C7754C6B9D9147E9397BD2E6', 'Group 1.1', 'B14F5D815B0D4ACC92B827CBBE39086B'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = '7E9FD972C7754C6B9D9147E9397BD2E6' AND Name = 'Group 1.1' AND ParentId = 'B14F5D815B0D4ACC92B827CBBE39086B'
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT 'DC195A4F46F941B280D277FF9C6269B7', 'Group 1.2', 'B14F5D815B0D4ACC92B827CBBE39086B'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = 'DC195A4F46F941B280D277FF9C6269B7' AND Name = 'Group 1.2' AND ParentId = 'B14F5D815B0D4ACC92B827CBBE39086B'
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT '72D461B2234B40D6B410B261964BA291', 'Group 2', 'F6E34BDFB76942DDA2BEFEE67FAF9045'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = '72D461B2234B40D6B410B261964BA291' AND Name = 'Group 2' AND ParentId = 'F6E34BDFB76942DDA2BEFEE67FAF9045'
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT 'C5DCC1489C0C45C48A68901D99A26184', 'Group 2.2', '72D461B2234B40D6B410B261964BA291'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = 'C5DCC1489C0C45C48A68901D99A26184' AND Name = 'Group 2.2' AND ParentId = '72D461B2234B40D6B410B261964BA291'
);
INSERT INTO STRUCTDIVISION(Id, Name, ParentId)
SELECT 'BC21A48228E749518177E57813A70FC5', 'Group 2.1', '72D461B2234B40D6B410B261964BA291'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM STRUCTDIVISION WHERE Id = 'BC21A48228E749518177E57813A70FC5' AND Name = 'Group 2.1' AND ParentId = '72D461B2234B40D6B410B261964BA291'
);


INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT '81537E2191C54811A5462DDDFF6BF409', 'Silviya', 'F6E34BDFB76942DDA2BEFEE67FAF9045', 1
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = '81537E2191C54811A5462DDDFF6BF409' AND Name = 'Silviya' AND StructDivisionId = 'F6E34BDFB76942DDA2BEFEE67FAF9045' AND IsHead = 1
);
INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT 'B0E6FD4C2DB94BB6A62E68B6B8999905', 'Margo', 'DC195A4F46F941B280D277FF9C6269B7', 0
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = 'B0E6FD4C2DB94BB6A62E68B6B8999905' AND Name = 'Margo' AND StructDivisionId = 'DC195A4F46F941B280D277FF9C6269B7' AND IsHead = 0
);
INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT 'DEB579F9991C4DB9A17DBB1ECCF2842C', 'Max', 'B14F5D815B0D4ACC92B827CBBE39086B', 1
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = 'DEB579F9991C4DB9A17DBB1ECCF2842C' AND Name = 'Max' AND StructDivisionId = 'B14F5D815B0D4ACC92B827CBBE39086B' AND IsHead = 1
);
INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT '91F2B4714A964AB7A41AEA4293703D16', 'John', '7E9FD972C7754C6B9D9147E9397BD2E6', 1
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = '91F2B4714A964AB7A41AEA4293703D16' AND Name = 'John' AND StructDivisionId = '7E9FD972C7754C6B9D9147E9397BD2E6' AND IsHead = 1
);
INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT 'E41B48E3C03D484F87641711248C4F8A', 'Maria', 'C5DCC1489C0C45C48A68901D99A26184', 0
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = 'E41B48E3C03D484F87641711248C4F8A' AND Name = 'Maria' AND StructDivisionId = 'C5DCC1489C0C45C48A68901D99A26184' AND IsHead = 0
);
INSERT INTO EMPLOYEE(Id, Name, StructDivisionId, IsHead)
SELECT 'BBE686F8873648A7A8862DA25567F978', 'Mark', '7E9FD972C7754C6B9D9147E9397BD2E6', 0
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEE WHERE Id = 'BBE686F8873648A7A8862DA25567F978' AND Name = 'Mark' AND StructDivisionId = '7E9FD972C7754C6B9D9147E9397BD2E6' AND IsHead = 0
);

INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT 'E41B48E3C03D484F87641711248C4F8A', '412174C204904101A7B3830DE90BCAA0'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = 'E41B48E3C03D484F87641711248C4F8A' AND ROLEID = '412174C204904101A7B3830DE90BCAA0'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT 'E41B48E3C03D484F87641711248C4F8A', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = 'E41B48E3C03D484F87641711248C4F8A' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT 'BBE686F8873648A7A8862DA25567F978', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = 'BBE686F8873648A7A8862DA25567F978' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT '81537E2191C54811A5462DDDFF6BF409', '8D378EBE066646B3B7AB1A52480FD12A'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = '81537E2191C54811A5462DDDFF6BF409' AND ROLEID = '8D378EBE066646B3B7AB1A52480FD12A'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT '81537E2191C54811A5462DDDFF6BF409', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = '81537E2191C54811A5462DDDFF6BF409' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT 'B0E6FD4C2DB94BB6A62E68B6B8999905', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = 'B0E6FD4C2DB94BB6A62E68B6B8999905' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT 'DEB579F9991C4DB9A17DBB1ECCF2842C', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = 'DEB579F9991C4DB9A17DBB1ECCF2842C' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);
INSERT INTO EMPLOYEEROLE(EMPLOYEEID, ROLEID)
SELECT '91F2B4714A964AB7A41AEA4293703D16', '71FFFB5BB7074B3C951CC37FDFCC8DFB'
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM EMPLOYEEROLE WHERE EMPLOYEEID = '91F2B4714A964AB7A41AEA4293703D16' AND ROLEID = '71FFFB5BB7074B3C951CC37FDFCC8DFB'
);


DECLARE
    schemeValue varchar2(32767);
BEGIN
schemeValue := '
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
';
INSERT INTO WorkflowScheme(Code, Scheme)
SELECT 'SimpleWF', schemeValue
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM WorkflowScheme WHERE Code = 'SimpleWF'
);
END;

END;