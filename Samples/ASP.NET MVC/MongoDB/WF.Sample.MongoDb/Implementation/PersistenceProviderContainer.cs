﻿using System;
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
          Scheme = @"
<Process Name=""SimpleWF"">
  <Designer X=""-110"" Y=""-60"" />
  <Actors>
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
    <Activity Name=""VacationRequestCreated"" State=""VacationRequestCreated"" IsInitial=""true"" IsFinal=""false"" IsForSetState=""true"" IsAutoSchemeUpdate=""true"" DisablePersist=""false"">
      <Designer X=""20"" Y=""160"" Hidden=""false"" />
    </Activity>
    <Activity Name=""ManagerSigning"" State=""ManagerSigning"" IsInitial=""false"" IsFinal=""false"" IsForSetState=""true"" IsAutoSchemeUpdate=""true"" DisablePersist=""false"">
      <Designer X=""360"" Y=""160"" Hidden=""false"" />
    </Activity>
    <Activity Name=""BigBossSigning"" State=""BigBossSigning"" IsInitial=""false"" IsFinal=""false"" IsForSetState=""true"" IsAutoSchemeUpdate=""true"" DisablePersist=""false"">
      <Designer X=""640"" Y=""160"" Hidden=""false"" />
    </Activity>
    <Activity Name=""AccountingReview"" State=""AccountingReview"" IsInitial=""false"" IsFinal=""false"" IsForSetState=""true"" IsAutoSchemeUpdate=""true"" DisablePersist=""false"">
      <Designer X=""630"" Y=""320"" Hidden=""false"" />
    </Activity>
    <Activity Name=""RequestApproved"" State=""RequestApproved"" IsInitial=""false"" IsFinal=""true"" IsForSetState=""true"" IsAutoSchemeUpdate=""true"" DisablePersist=""false"">
      <Designer X=""900"" Y=""320"" Hidden=""false"" />
    </Activity>
  </Activities>
  <Transitions>
    <Transition Name=""ManagerSigning_Draft_1"" To=""VacationRequestCreated"" From=""ManagerSigning"" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Manager"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Reject"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer X=""269"" Y=""205"" Hidden=""false"" />
    </Transition>
    <Transition Name=""BigBossSigning_Activity_1_1"" To=""AccountingReview"" From=""BigBossSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""BigBoss"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Approve"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer Hidden=""false"" />
    </Transition>
    <Transition Name=""ManagerSigning_Approved_1"" To=""AccountingReview"" From=""ManagerSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Manager"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Approve"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Otherwise"" />
      </Conditions>
      <Designer X=""453"" Y=""271"" Hidden=""false"" />
    </Transition>
    <Transition Name=""ManagerSigning_BigBossSigning_1"" To=""BigBossSigning"" From=""ManagerSigning"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Manager"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Approve"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Expression"" ConditionInversion=""false"">
          <Expression><![CDATA[@Sum > 100]]></Expression>
        </Condition>
      </Conditions>
      <Designer X=""562"" Y=""177"" Hidden=""false"" />
    </Transition>
    <Transition Name=""Draft_ManagerSigning_1"" To=""ManagerSigning"" From=""VacationRequestCreated"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Author"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""StartSigning"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer X=""269"" Y=""170"" Hidden=""false"" />
    </Transition>
    <Transition Name=""BigBossSigning_ManagerSigning_1"" To=""ManagerSigning"" From=""BigBossSigning"" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""BigBoss"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Reject"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer X=""563"" Y=""204"" Hidden=""false"" />
    </Transition>
    <Transition Name=""ManagerSigning_BigBossSigning_2"" To=""BigBossSigning"" From=""ManagerSigning"" Classifier=""NotSpecified"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Triggers>
        <Trigger Type=""Timer"" NameRef=""SendToBigBoss"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer X=""557"" Y=""120"" Hidden=""false"" />
    </Transition>
    <Transition Name=""Accountant_Activity_1_1"" To=""RequestApproved"" From=""AccountingReview"" Classifier=""Direct"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Accountant"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Paid"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer Hidden=""false"" />
    </Transition>
    <Transition Name=""Accountant_ManagerSigning_1"" To=""ManagerSigning"" From=""AccountingReview"" Classifier=""Reverse"" AllowConcatenationType=""And"" RestrictConcatenationType=""And"" ConditionsConcatenationType=""And"" DisableParentStateControl=""false"">
      <Restrictions>
        <Restriction Type=""Allow"" NameRef=""Accountant"" />
      </Restrictions>
      <Triggers>
        <Trigger Type=""Command"" NameRef=""Reject"" />
      </Triggers>
      <Conditions>
        <Condition Type=""Always"" />
      </Conditions>
      <Designer X=""395"" Y=""343"" Hidden=""false"" />
    </Transition>
  </Transitions>
  <Localization>
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""ManagerSigning"" Value=""Manager signing"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""BigBossSigning"" Value=""BigBoss signing"" />
    <Localize Type=""Command"" IsDefault=""True"" Culture=""en-US"" ObjectName=""StartSigning"" Value=""Start signing"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""AccountingReview"" Value=""Accounting review"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""VacationRequestCreated"" Value=""Vacation request created"" />
    <Localize Type=""State"" IsDefault=""True"" Culture=""en-US"" ObjectName=""RequestApproved"" Value=""Request approved"" />
  </Localization>
</Process>
"
        });

        #endregion
      }
    }
  }
}
