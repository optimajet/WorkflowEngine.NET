using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Reflection;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace Budget2.Workflow
{
    partial class Demand
    {
        #region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        [System.CodeDom.Compiler.GeneratedCode("", "")]
        private void InitializeComponent()
        {
            this.CanModifyActivities = true;
            System.Workflow.Activities.CodeCondition codecondition1 = new System.Workflow.Activities.CodeCondition();
            this.setStateActivity1 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity9 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity4 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity3 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity5 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity14 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity13 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity16 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity12 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity10 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity6 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity4 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity3 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity8 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity2 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity2 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity6 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.AgreedInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity11 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity7 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity7 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity6 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity14 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity12 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity9 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity11 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZCuratorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity10 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity8 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity5 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity5 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.InitiatorHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity4 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity4 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity3 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity3 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.OPHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity8 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.denialEventFired1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity2 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity2 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.OPExpertSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity1 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.DraftInitCode = new System.Workflow.Activities.CodeActivity();
            this.AgreedInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZCuratorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.InitiatorHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.OPHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.OPExpertSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.StartProcessingEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.DraftInit = new System.Workflow.Activities.StateInitializationActivity();
            this.Archived = new System.Workflow.Activities.StateActivity();
            this.Agreed = new System.Workflow.Activities.StateActivity();
            this.UPKZHeadSighting = new System.Workflow.Activities.StateActivity();
            this.UPKZCuratorSighting = new System.Workflow.Activities.StateActivity();
            this.InitiatorHeadSighting = new System.Workflow.Activities.StateActivity();
            this.OPHeadSighting = new System.Workflow.Activities.StateActivity();
            this.OPExpertSighting = new System.Workflow.Activities.StateActivity();
            this.Draft = new System.Workflow.Activities.StateActivity();
            // 
            // setStateActivity1
            // 
            this.setStateActivity1.Name = "setStateActivity1";
            this.setStateActivity1.TargetStateName = "OPExpertSighting";
            // 
            // setStateActivity9
            // 
            this.setStateActivity9.Name = "setStateActivity9";
            this.setStateActivity9.TargetStateName = "OPHeadSighting";
            // 
            // ifElseBranchActivity4
            // 
            this.ifElseBranchActivity4.Activities.Add(this.setStateActivity1);
            this.ifElseBranchActivity4.Name = "ifElseBranchActivity4";
            // 
            // ifElseBranchActivity3
            // 
            this.ifElseBranchActivity3.Activities.Add(this.setStateActivity9);
            codecondition1.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckInitiatorIsExecutorStructDivision_ExecuteCode);
            this.ifElseBranchActivity3.Condition = codecondition1;
            this.ifElseBranchActivity3.Name = "ifElseBranchActivity3";
            // 
            // setStateActivity5
            // 
            this.setStateActivity5.Name = "setStateActivity5";
            this.setStateActivity5.TargetStateName = "Archived";
            // 
            // setStateActivity14
            // 
            this.setStateActivity14.Name = "setStateActivity14";
            this.setStateActivity14.TargetStateName = "Draft";
            // 
            // setStateActivity13
            // 
            this.setStateActivity13.Name = "setStateActivity13";
            this.setStateActivity13.TargetStateName = "Agreed";
            // 
            // setStateActivity16
            // 
            this.setStateActivity16.Name = "setStateActivity16";
            this.setStateActivity16.TargetStateName = "Draft";
            // 
            // setStateActivity12
            // 
            this.setStateActivity12.Name = "setStateActivity12";
            this.setStateActivity12.TargetStateName = "UPKZHeadSighting";
            // 
            // setStateActivity10
            // 
            this.setStateActivity10.Name = "setStateActivity10";
            this.setStateActivity10.TargetStateName = "Draft";
            // 
            // setStateActivity6
            // 
            this.setStateActivity6.Name = "setStateActivity6";
            this.setStateActivity6.TargetStateName = "OPHeadSighting";
            // 
            // setStateActivity4
            // 
            this.setStateActivity4.Name = "setStateActivity4";
            this.setStateActivity4.TargetStateName = "Draft";
            // 
            // setStateActivity3
            // 
            this.setStateActivity3.Name = "setStateActivity3";
            this.setStateActivity3.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity8
            // 
            this.setStateActivity8.Name = "setStateActivity8";
            this.setStateActivity8.TargetStateName = "Draft";
            // 
            // setStateActivity2
            // 
            this.setStateActivity2.Name = "setStateActivity2";
            this.setStateActivity2.TargetStateName = "InitiatorHeadSighting";
            // 
            // ifElseActivity2
            // 
            this.ifElseActivity2.Activities.Add(this.ifElseBranchActivity3);
            this.ifElseActivity2.Activities.Add(this.ifElseBranchActivity4);
            this.ifElseActivity2.Name = "ifElseActivity2";
            // 
            // transactionScopeActivity6
            // 
            this.transactionScopeActivity6.Activities.Add(this.setStateActivity5);
            this.transactionScopeActivity6.Name = "transactionScopeActivity6";
            this.transactionScopeActivity6.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // AgreedInitCode
            // 
            this.AgreedInitCode.Name = "AgreedInitCode";
            this.AgreedInitCode.ExecuteCode += new System.EventHandler(this.AgreedInitCode_ExecuteCode);
            // 
            // transactionScopeActivity11
            // 
            this.transactionScopeActivity11.Activities.Add(this.setStateActivity14);
            this.transactionScopeActivity11.Name = "transactionScopeActivity11";
            this.transactionScopeActivity11.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity7
            // 
            this.handleExternalEventActivity7.EventName = "Denial";
            this.handleExternalEventActivity7.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity7.Name = "handleExternalEventActivity7";
            this.handleExternalEventActivity7.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity7
            // 
            this.transactionScopeActivity7.Activities.Add(this.setStateActivity13);
            this.transactionScopeActivity7.Name = "transactionScopeActivity7";
            this.transactionScopeActivity7.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity6
            // 
            this.handleExternalEventActivity6.EventName = "Sighting";
            this.handleExternalEventActivity6.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity6.Name = "handleExternalEventActivity6";
            this.handleExternalEventActivity6.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // UPKZHeadSightingInitCode
            // 
            this.UPKZHeadSightingInitCode.Name = "UPKZHeadSightingInitCode";
            this.UPKZHeadSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZHeadSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity14
            // 
            this.transactionScopeActivity14.Activities.Add(this.setStateActivity16);
            this.transactionScopeActivity14.Name = "transactionScopeActivity14";
            this.transactionScopeActivity14.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity12
            // 
            this.handleExternalEventActivity12.EventName = "Denial";
            this.handleExternalEventActivity12.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity12.Name = "handleExternalEventActivity12";
            this.handleExternalEventActivity12.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity9
            // 
            this.transactionScopeActivity9.Activities.Add(this.setStateActivity12);
            this.transactionScopeActivity9.Name = "transactionScopeActivity9";
            this.transactionScopeActivity9.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity11
            // 
            this.handleExternalEventActivity11.EventName = "Sighting";
            this.handleExternalEventActivity11.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity11.Name = "handleExternalEventActivity11";
            this.handleExternalEventActivity11.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // UPKZCuratorSightingInitCode
            // 
            this.UPKZCuratorSightingInitCode.Name = "UPKZCuratorSightingInitCode";
            this.UPKZCuratorSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZCuratorSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity10
            // 
            this.transactionScopeActivity10.Activities.Add(this.setStateActivity10);
            this.transactionScopeActivity10.Name = "transactionScopeActivity10";
            this.transactionScopeActivity10.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity8
            // 
            this.handleExternalEventActivity8.EventName = "Denial";
            this.handleExternalEventActivity8.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity8.Name = "handleExternalEventActivity8";
            this.handleExternalEventActivity8.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity5
            // 
            this.transactionScopeActivity5.Activities.Add(this.setStateActivity6);
            this.transactionScopeActivity5.Name = "transactionScopeActivity5";
            this.transactionScopeActivity5.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity5
            // 
            this.handleExternalEventActivity5.EventName = "Sighting";
            this.handleExternalEventActivity5.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity5.Name = "handleExternalEventActivity5";
            this.handleExternalEventActivity5.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // InitiatorHeadSightingInitCode
            // 
            this.InitiatorHeadSightingInitCode.Name = "InitiatorHeadSightingInitCode";
            this.InitiatorHeadSightingInitCode.ExecuteCode += new System.EventHandler(this.InitiatorHeadSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity4
            // 
            this.transactionScopeActivity4.Activities.Add(this.setStateActivity4);
            this.transactionScopeActivity4.Name = "transactionScopeActivity4";
            this.transactionScopeActivity4.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity4
            // 
            this.handleExternalEventActivity4.EventName = "Denial";
            this.handleExternalEventActivity4.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity4.Name = "handleExternalEventActivity4";
            this.handleExternalEventActivity4.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity3
            // 
            this.transactionScopeActivity3.Activities.Add(this.setStateActivity3);
            this.transactionScopeActivity3.Name = "transactionScopeActivity3";
            this.transactionScopeActivity3.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity3
            // 
            this.handleExternalEventActivity3.EventName = "Sighting";
            this.handleExternalEventActivity3.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity3.Name = "handleExternalEventActivity3";
            this.handleExternalEventActivity3.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // OPHeadSightingInitCode
            // 
            this.OPHeadSightingInitCode.Name = "OPHeadSightingInitCode";
            this.OPHeadSightingInitCode.ExecuteCode += new System.EventHandler(this.OPHeadSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity8
            // 
            this.transactionScopeActivity8.Activities.Add(this.setStateActivity8);
            this.transactionScopeActivity8.Name = "transactionScopeActivity8";
            this.transactionScopeActivity8.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // denialEventFired1
            // 
            this.denialEventFired1.EventName = "Denial";
            this.denialEventFired1.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.denialEventFired1.Name = "denialEventFired1";
            this.denialEventFired1.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity2
            // 
            this.transactionScopeActivity2.Activities.Add(this.setStateActivity2);
            this.transactionScopeActivity2.Name = "transactionScopeActivity2";
            this.transactionScopeActivity2.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity2
            // 
            this.handleExternalEventActivity2.EventName = "Sighting";
            this.handleExternalEventActivity2.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity2.Name = "handleExternalEventActivity2";
            this.handleExternalEventActivity2.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // OPExpertSightingInitCode
            // 
            this.OPExpertSightingInitCode.Name = "OPExpertSightingInitCode";
            this.OPExpertSightingInitCode.ExecuteCode += new System.EventHandler(this.OPExpertSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity1
            // 
            this.transactionScopeActivity1.Activities.Add(this.ifElseActivity2);
            this.transactionScopeActivity1.Name = "transactionScopeActivity1";
            this.transactionScopeActivity1.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity1
            // 
            this.handleExternalEventActivity1.EventName = "StartProcessing";
            this.handleExternalEventActivity1.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity1.Name = "handleExternalEventActivity1";
            this.handleExternalEventActivity1.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // DraftInitCode
            // 
            this.DraftInitCode.Name = "DraftInitCode";
            this.DraftInitCode.ExecuteCode += new System.EventHandler(this.DraftInitCode_ExecuteCode);
            // 
            // AgreedInit
            // 
            this.AgreedInit.Activities.Add(this.AgreedInitCode);
            this.AgreedInit.Activities.Add(this.transactionScopeActivity6);
            this.AgreedInit.Name = "AgreedInit";
            // 
            // DenialEvent5
            // 
            this.DenialEvent5.Activities.Add(this.handleExternalEventActivity7);
            this.DenialEvent5.Activities.Add(this.transactionScopeActivity11);
            this.DenialEvent5.Name = "DenialEvent5";
            // 
            // SightingEvent5
            // 
            this.SightingEvent5.Activities.Add(this.handleExternalEventActivity6);
            this.SightingEvent5.Activities.Add(this.transactionScopeActivity7);
            this.SightingEvent5.Name = "SightingEvent5";
            // 
            // UPKZHeadSightingInit
            // 
            this.UPKZHeadSightingInit.Activities.Add(this.UPKZHeadSightingInitCode);
            this.UPKZHeadSightingInit.Name = "UPKZHeadSightingInit";
            // 
            // DenialEvent4
            // 
            this.DenialEvent4.Activities.Add(this.handleExternalEventActivity12);
            this.DenialEvent4.Activities.Add(this.transactionScopeActivity14);
            this.DenialEvent4.Name = "DenialEvent4";
            // 
            // SightingEvent4
            // 
            this.SightingEvent4.Activities.Add(this.handleExternalEventActivity11);
            this.SightingEvent4.Activities.Add(this.transactionScopeActivity9);
            this.SightingEvent4.Name = "SightingEvent4";
            // 
            // UPKZCuratorSightingInit
            // 
            this.UPKZCuratorSightingInit.Activities.Add(this.UPKZCuratorSightingInitCode);
            this.UPKZCuratorSightingInit.Name = "UPKZCuratorSightingInit";
            // 
            // DenialEvent3
            // 
            this.DenialEvent3.Activities.Add(this.handleExternalEventActivity8);
            this.DenialEvent3.Activities.Add(this.transactionScopeActivity10);
            this.DenialEvent3.Name = "DenialEvent3";
            // 
            // SightingEvent3
            // 
            this.SightingEvent3.Activities.Add(this.handleExternalEventActivity5);
            this.SightingEvent3.Activities.Add(this.transactionScopeActivity5);
            this.SightingEvent3.Name = "SightingEvent3";
            // 
            // InitiatorHeadSightingInit
            // 
            this.InitiatorHeadSightingInit.Activities.Add(this.InitiatorHeadSightingInitCode);
            this.InitiatorHeadSightingInit.Name = "InitiatorHeadSightingInit";
            // 
            // DenialEvent2
            // 
            this.DenialEvent2.Activities.Add(this.handleExternalEventActivity4);
            this.DenialEvent2.Activities.Add(this.transactionScopeActivity4);
            this.DenialEvent2.Name = "DenialEvent2";
            // 
            // SightingEvent2
            // 
            this.SightingEvent2.Activities.Add(this.handleExternalEventActivity3);
            this.SightingEvent2.Activities.Add(this.transactionScopeActivity3);
            this.SightingEvent2.Name = "SightingEvent2";
            // 
            // OPHeadSightingInit
            // 
            this.OPHeadSightingInit.Activities.Add(this.OPHeadSightingInitCode);
            this.OPHeadSightingInit.Name = "OPHeadSightingInit";
            // 
            // DenialEvent1
            // 
            this.DenialEvent1.Activities.Add(this.denialEventFired1);
            this.DenialEvent1.Activities.Add(this.transactionScopeActivity8);
            this.DenialEvent1.Name = "DenialEvent1";
            // 
            // SightingEvent1
            // 
            this.SightingEvent1.Activities.Add(this.handleExternalEventActivity2);
            this.SightingEvent1.Activities.Add(this.transactionScopeActivity2);
            this.SightingEvent1.Name = "SightingEvent1";
            // 
            // OPExpertSightingInit
            // 
            this.OPExpertSightingInit.Activities.Add(this.OPExpertSightingInitCode);
            this.OPExpertSightingInit.Name = "OPExpertSightingInit";
            // 
            // StartProcessingEvent
            // 
            this.StartProcessingEvent.Activities.Add(this.handleExternalEventActivity1);
            this.StartProcessingEvent.Activities.Add(this.transactionScopeActivity1);
            this.StartProcessingEvent.Name = "StartProcessingEvent";
            // 
            // DraftInit
            // 
            this.DraftInit.Activities.Add(this.DraftInitCode);
            this.DraftInit.Name = "DraftInit";
            // 
            // Archived
            // 
            this.Archived.Name = "Archived";
            // 
            // Agreed
            // 
            this.Agreed.Activities.Add(this.AgreedInit);
            this.Agreed.Name = "Agreed";
            // 
            // UPKZHeadSighting
            // 
            this.UPKZHeadSighting.Activities.Add(this.UPKZHeadSightingInit);
            this.UPKZHeadSighting.Activities.Add(this.SightingEvent5);
            this.UPKZHeadSighting.Activities.Add(this.DenialEvent5);
            this.UPKZHeadSighting.Name = "UPKZHeadSighting";
            // 
            // UPKZCuratorSighting
            // 
            this.UPKZCuratorSighting.Activities.Add(this.UPKZCuratorSightingInit);
            this.UPKZCuratorSighting.Activities.Add(this.SightingEvent4);
            this.UPKZCuratorSighting.Activities.Add(this.DenialEvent4);
            this.UPKZCuratorSighting.Name = "UPKZCuratorSighting";
            // 
            // InitiatorHeadSighting
            // 
            this.InitiatorHeadSighting.Activities.Add(this.InitiatorHeadSightingInit);
            this.InitiatorHeadSighting.Activities.Add(this.SightingEvent3);
            this.InitiatorHeadSighting.Activities.Add(this.DenialEvent3);
            this.InitiatorHeadSighting.Name = "InitiatorHeadSighting";
            // 
            // OPHeadSighting
            // 
            this.OPHeadSighting.Activities.Add(this.OPHeadSightingInit);
            this.OPHeadSighting.Activities.Add(this.SightingEvent2);
            this.OPHeadSighting.Activities.Add(this.DenialEvent2);
            this.OPHeadSighting.Name = "OPHeadSighting";
            // 
            // OPExpertSighting
            // 
            this.OPExpertSighting.Activities.Add(this.OPExpertSightingInit);
            this.OPExpertSighting.Activities.Add(this.SightingEvent1);
            this.OPExpertSighting.Activities.Add(this.DenialEvent1);
            this.OPExpertSighting.Name = "OPExpertSighting";
            // 
            // Draft
            // 
            this.Draft.Activities.Add(this.DraftInit);
            this.Draft.Activities.Add(this.StartProcessingEvent);
            this.Draft.Name = "Draft";
            // 
            // Demand
            // 
            this.Activities.Add(this.Draft);
            this.Activities.Add(this.OPExpertSighting);
            this.Activities.Add(this.OPHeadSighting);
            this.Activities.Add(this.InitiatorHeadSighting);
            this.Activities.Add(this.UPKZCuratorSighting);
            this.Activities.Add(this.UPKZHeadSighting);
            this.Activities.Add(this.Agreed);
            this.Activities.Add(this.Archived);
            this.CompletedStateName = "Archived";
            this.DynamicUpdateCondition = null;
            this.InitialStateName = "Draft";
            this.Name = "Demand";
            this.CanModifyActivities = false;

        }

        #endregion

        private CodeActivity DraftInitCode;

        private StateInitializationActivity DraftInit;

        private CodeActivity OPExpertSightingInitCode;

        private StateInitializationActivity OPExpertSightingInit;

        private StateActivity OPHeadSighting;

        private StateActivity OPExpertSighting;

        private CodeActivity OPHeadSightingInitCode;

        private StateInitializationActivity OPHeadSightingInit;

        private StateActivity InitiatorHeadSighting;

        private CodeActivity InitiatorHeadSightingInitCode;

        private StateInitializationActivity InitiatorHeadSightingInit;

        private CodeActivity UPKZCuratorSightingInitCode;

        private StateInitializationActivity UPKZCuratorSightingInit;

        private StateActivity UPKZCuratorSighting;

        private CodeActivity UPKZHeadSightingInitCode;

        private StateInitializationActivity UPKZHeadSightingInit;

        private StateActivity UPKZHeadSighting;

        private StateActivity Agreed;

        private CodeActivity AgreedInitCode;

        private StateInitializationActivity AgreedInit;

        private StateActivity Archived;

        private SetStateActivity setStateActivity1;

        private SetStateActivity setStateActivity9;

        private IfElseBranchActivity ifElseBranchActivity4;

        private IfElseBranchActivity ifElseBranchActivity3;

        private IfElseActivity ifElseActivity2;

        private TransactionScopeActivity transactionScopeActivity1;

        private HandleExternalEventActivity handleExternalEventActivity1;

        private EventDrivenActivity StartProcessingEvent;

        private SetStateActivity setStateActivity8;

        private SetStateActivity setStateActivity2;

        private TransactionScopeActivity transactionScopeActivity8;

        private HandleExternalEventActivity denialEventFired1;

        private TransactionScopeActivity transactionScopeActivity2;

        private HandleExternalEventActivity handleExternalEventActivity2;

        private EventDrivenActivity DenialEvent1;

        private EventDrivenActivity SightingEvent1;

        private SetStateActivity setStateActivity4;

        private SetStateActivity setStateActivity3;

        private TransactionScopeActivity transactionScopeActivity4;

        private HandleExternalEventActivity handleExternalEventActivity4;

        private TransactionScopeActivity transactionScopeActivity3;

        private HandleExternalEventActivity handleExternalEventActivity3;

        private EventDrivenActivity DenialEvent2;

        private EventDrivenActivity SightingEvent2;

        private SetStateActivity setStateActivity10;

        private SetStateActivity setStateActivity6;

        private TransactionScopeActivity transactionScopeActivity10;

        private HandleExternalEventActivity handleExternalEventActivity8;

        private TransactionScopeActivity transactionScopeActivity5;

        private HandleExternalEventActivity handleExternalEventActivity5;

        private EventDrivenActivity DenialEvent3;

        private EventDrivenActivity SightingEvent3;

        private SetStateActivity setStateActivity5;

        private SetStateActivity setStateActivity16;

        private SetStateActivity setStateActivity12;

        private TransactionScopeActivity transactionScopeActivity6;

        private TransactionScopeActivity transactionScopeActivity14;

        private HandleExternalEventActivity handleExternalEventActivity12;

        private TransactionScopeActivity transactionScopeActivity9;

        private HandleExternalEventActivity handleExternalEventActivity11;

        private EventDrivenActivity DenialEvent4;

        private EventDrivenActivity SightingEvent4;

        private SetStateActivity setStateActivity14;

        private SetStateActivity setStateActivity13;

        private TransactionScopeActivity transactionScopeActivity11;

        private HandleExternalEventActivity handleExternalEventActivity7;

        private TransactionScopeActivity transactionScopeActivity7;

        private HandleExternalEventActivity handleExternalEventActivity6;

        private EventDrivenActivity DenialEvent5;

        private EventDrivenActivity SightingEvent5;

        private StateActivity Draft;






















    }
}
