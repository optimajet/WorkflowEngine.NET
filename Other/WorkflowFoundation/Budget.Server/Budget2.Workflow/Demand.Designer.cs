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
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference1 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference2 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference3 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference4 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference5 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference6 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.Activities.CodeCondition codecondition1 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition2 = new System.Workflow.Activities.CodeCondition();
            this.setStateActivity38 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity37 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity36 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity35 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity34 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity31 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity3 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity7 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity1 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity9 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity14 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity13 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity12 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity11 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity10 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity8 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity4 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity3 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseActivity4 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity30 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity22 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity19 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity11 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity5 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity29 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity14 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity13 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity28 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity16 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity12 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity26 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity10 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity6 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity27 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity4 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity1 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity25 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity8 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity2 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity2 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity28 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity26 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity27 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity25 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.RollbackRequestedInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity19 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity17 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity16 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity14 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity12 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity9 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.AgreementOPExpertSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity6 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.AgreedInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity26 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity24 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity11 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity7 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity7 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity6 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity25 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity23 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity14 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity12 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity9 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity11 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZCuratorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity23 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity21 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity10 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity8 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity5 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity5 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.InitiatorHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity24 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity22 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity4 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity4 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity3 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity3 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.OPHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity22 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity20 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity8 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.denialEventFired1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity2 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity2 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.OPExpertSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity1 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.DraftInitCode = new System.Workflow.Activities.CodeActivity();
            this.handleExternalEventActivity27 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.eventDrivenActivity16 = new System.Workflow.Activities.EventDrivenActivity();
            this.eventDrivenActivity15 = new System.Workflow.Activities.EventDrivenActivity();
            this.RollbackRequestedInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity7 = new System.Workflow.Activities.EventDrivenActivity();
            this.eventDrivenActivity4 = new System.Workflow.Activities.EventDrivenActivity();
            this.eventDrivenActivity1 = new System.Workflow.Activities.EventDrivenActivity();
            this.AgreementOPExpertSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.AgreedInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity14 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity13 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZCuratorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity11 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.InitiatorHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity12 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.OPHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity10 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.OPExpertSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.StartProcessingEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.DraftInit = new System.Workflow.Activities.StateInitializationActivity();
            this.eventDrivenActivity17 = new System.Workflow.Activities.EventDrivenActivity();
            this.RollbackRequested = new System.Workflow.Activities.StateActivity();
            this.AgreementOPExpertSighting = new System.Workflow.Activities.StateActivity();
            this.Archived = new System.Workflow.Activities.StateActivity();
            this.Agreed = new System.Workflow.Activities.StateActivity();
            this.UPKZHeadSighting = new System.Workflow.Activities.StateActivity();
            this.UPKZCuratorSighting = new System.Workflow.Activities.StateActivity();
            this.InitiatorHeadSighting = new System.Workflow.Activities.StateActivity();
            this.OPHeadSighting = new System.Workflow.Activities.StateActivity();
            this.OPExpertSighting = new System.Workflow.Activities.StateActivity();
            this.Draft = new System.Workflow.Activities.StateActivity();
            // 
            // setStateActivity38
            // 
            this.setStateActivity38.Name = "setStateActivity38";
            this.setStateActivity38.TargetStateName = "UPKZHeadSighting";
            // 
            // setStateActivity37
            // 
            this.setStateActivity37.Name = "setStateActivity37";
            this.setStateActivity37.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity36
            // 
            this.setStateActivity36.Name = "setStateActivity36";
            this.setStateActivity36.TargetStateName = "OPExpertSighting";
            // 
            // setStateActivity35
            // 
            this.setStateActivity35.Name = "setStateActivity35";
            this.setStateActivity35.TargetStateName = "OPHeadSighting";
            // 
            // setStateActivity34
            // 
            this.setStateActivity34.Name = "setStateActivity34";
            this.setStateActivity34.TargetStateName = "InitiatorHeadSighting";
            // 
            // setStateActivity31
            // 
            this.setStateActivity31.Name = "setStateActivity31";
            this.setStateActivity31.TargetStateName = "AgreementOPExpertSighting";
            // 
            // setStateActivity3
            // 
            this.setStateActivity3.Name = "setStateActivity3";
            this.setStateActivity3.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity7
            // 
            this.setStateActivity7.Name = "setStateActivity7";
            this.setStateActivity7.TargetStateName = "AgreementOPExpertSighting";
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
            // ifElseBranchActivity14
            // 
            this.ifElseBranchActivity14.Activities.Add(this.setStateActivity38);
            ruleconditionreference1.ConditionName = "ifElseBranchActivity14c";
            this.ifElseBranchActivity14.Condition = ruleconditionreference1;
            this.ifElseBranchActivity14.Name = "ifElseBranchActivity14";
            // 
            // ifElseBranchActivity13
            // 
            this.ifElseBranchActivity13.Activities.Add(this.setStateActivity37);
            ruleconditionreference2.ConditionName = "ifElseBranchActivity13c";
            this.ifElseBranchActivity13.Condition = ruleconditionreference2;
            this.ifElseBranchActivity13.Name = "ifElseBranchActivity13";
            // 
            // ifElseBranchActivity12
            // 
            this.ifElseBranchActivity12.Activities.Add(this.setStateActivity36);
            ruleconditionreference3.ConditionName = "ifElseBranchActivity12c";
            this.ifElseBranchActivity12.Condition = ruleconditionreference3;
            this.ifElseBranchActivity12.Name = "ifElseBranchActivity12";
            // 
            // ifElseBranchActivity11
            // 
            this.ifElseBranchActivity11.Activities.Add(this.setStateActivity35);
            ruleconditionreference4.ConditionName = "ifElseBranchActivity11c";
            this.ifElseBranchActivity11.Condition = ruleconditionreference4;
            this.ifElseBranchActivity11.Name = "ifElseBranchActivity11";
            // 
            // ifElseBranchActivity10
            // 
            this.ifElseBranchActivity10.Activities.Add(this.setStateActivity34);
            ruleconditionreference5.ConditionName = "ifElseBranchActivity10c";
            this.ifElseBranchActivity10.Condition = ruleconditionreference5;
            this.ifElseBranchActivity10.Name = "ifElseBranchActivity10";
            // 
            // ifElseBranchActivity8
            // 
            this.ifElseBranchActivity8.Activities.Add(this.setStateActivity31);
            ruleconditionreference6.ConditionName = "ifElseBranchActivity8c";
            this.ifElseBranchActivity8.Condition = ruleconditionreference6;
            this.ifElseBranchActivity8.Name = "ifElseBranchActivity8";
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.setStateActivity3);
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.setStateActivity7);
            codecondition1.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckSendToAgreementStructDivision_ExecuteCode);
            this.ifElseBranchActivity1.Condition = codecondition1;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // ifElseBranchActivity4
            // 
            this.ifElseBranchActivity4.Activities.Add(this.setStateActivity1);
            this.ifElseBranchActivity4.Name = "ifElseBranchActivity4";
            // 
            // ifElseBranchActivity3
            // 
            this.ifElseBranchActivity3.Activities.Add(this.setStateActivity9);
            codecondition2.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckInitiatorIsExecutorStructDivision_ExecuteCode);
            this.ifElseBranchActivity3.Condition = codecondition2;
            this.ifElseBranchActivity3.Name = "ifElseBranchActivity3";
            // 
            // ifElseActivity4
            // 
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity8);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity10);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity11);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity12);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity13);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity14);
            this.ifElseActivity4.Name = "ifElseActivity4";
            // 
            // setStateActivity30
            // 
            this.setStateActivity30.Name = "setStateActivity30";
            this.setStateActivity30.TargetStateName = "Draft";
            // 
            // setStateActivity22
            // 
            this.setStateActivity22.Name = "setStateActivity22";
            this.setStateActivity22.TargetStateName = "RollbackRequested";
            // 
            // setStateActivity19
            // 
            this.setStateActivity19.Name = "setStateActivity19";
            this.setStateActivity19.TargetStateName = "Draft";
            // 
            // setStateActivity11
            // 
            this.setStateActivity11.Name = "setStateActivity11";
            this.setStateActivity11.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity5
            // 
            this.setStateActivity5.Name = "setStateActivity5";
            this.setStateActivity5.TargetStateName = "Archived";
            // 
            // setStateActivity29
            // 
            this.setStateActivity29.Name = "setStateActivity29";
            this.setStateActivity29.TargetStateName = "RollbackRequested";
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
            // setStateActivity28
            // 
            this.setStateActivity28.Name = "setStateActivity28";
            this.setStateActivity28.TargetStateName = "RollbackRequested";
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
            // setStateActivity26
            // 
            this.setStateActivity26.Name = "setStateActivity26";
            this.setStateActivity26.TargetStateName = "RollbackRequested";
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
            // setStateActivity27
            // 
            this.setStateActivity27.Name = "setStateActivity27";
            this.setStateActivity27.TargetStateName = "RollbackRequested";
            // 
            // setStateActivity4
            // 
            this.setStateActivity4.Name = "setStateActivity4";
            this.setStateActivity4.TargetStateName = "Draft";
            // 
            // ifElseActivity1
            // 
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity1);
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity2);
            this.ifElseActivity1.Name = "ifElseActivity1";
            // 
            // setStateActivity25
            // 
            this.setStateActivity25.Name = "setStateActivity25";
            this.setStateActivity25.TargetStateName = "RollbackRequested";
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
            // transactionScopeActivity28
            // 
            this.transactionScopeActivity28.Activities.Add(this.ifElseActivity4);
            this.transactionScopeActivity28.Name = "transactionScopeActivity28";
            this.transactionScopeActivity28.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity26
            // 
            this.handleExternalEventActivity26.EventName = "Denial";
            this.handleExternalEventActivity26.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity26.Name = "handleExternalEventActivity26";
            this.handleExternalEventActivity26.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity27
            // 
            this.transactionScopeActivity27.Activities.Add(this.setStateActivity30);
            this.transactionScopeActivity27.Name = "transactionScopeActivity27";
            this.transactionScopeActivity27.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity25
            // 
            this.handleExternalEventActivity25.EventName = "Sighting";
            this.handleExternalEventActivity25.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity25.Name = "handleExternalEventActivity25";
            this.handleExternalEventActivity25.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // RollbackRequestedInitCode
            // 
            this.RollbackRequestedInitCode.Name = "RollbackRequestedInitCode";
            this.RollbackRequestedInitCode.ExecuteCode += new System.EventHandler(this.RollbackRequestedInitCode_ExecuteCode);
            // 
            // transactionScopeActivity19
            // 
            this.transactionScopeActivity19.Activities.Add(this.setStateActivity22);
            this.transactionScopeActivity19.Name = "transactionScopeActivity19";
            this.transactionScopeActivity19.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity17
            // 
            this.handleExternalEventActivity17.EventName = "Rollback";
            this.handleExternalEventActivity17.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity17.Name = "handleExternalEventActivity17";
            this.handleExternalEventActivity17.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
            // 
            // transactionScopeActivity16
            // 
            this.transactionScopeActivity16.Activities.Add(this.setStateActivity19);
            this.transactionScopeActivity16.Name = "transactionScopeActivity16";
            this.transactionScopeActivity16.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity14
            // 
            this.handleExternalEventActivity14.EventName = "Denial";
            this.handleExternalEventActivity14.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity14.Name = "handleExternalEventActivity14";
            this.handleExternalEventActivity14.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity12
            // 
            this.transactionScopeActivity12.Activities.Add(this.setStateActivity11);
            this.transactionScopeActivity12.Name = "transactionScopeActivity12";
            this.transactionScopeActivity12.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity9
            // 
            this.handleExternalEventActivity9.EventName = "Sighting";
            this.handleExternalEventActivity9.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity9.Name = "handleExternalEventActivity9";
            this.handleExternalEventActivity9.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // AgreementOPExpertSightingInitCode
            // 
            this.AgreementOPExpertSightingInitCode.Name = "AgreementOPExpertSightingInitCode";
            this.AgreementOPExpertSightingInitCode.ExecuteCode += new System.EventHandler(this.AgreementOPExpertSightingInitCode_ExecuteCode);
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
            // transactionScopeActivity26
            // 
            this.transactionScopeActivity26.Activities.Add(this.setStateActivity29);
            this.transactionScopeActivity26.Name = "transactionScopeActivity26";
            this.transactionScopeActivity26.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity24
            // 
            this.handleExternalEventActivity24.EventName = "Rollback";
            this.handleExternalEventActivity24.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity24.Name = "handleExternalEventActivity24";
            this.handleExternalEventActivity24.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
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
            // transactionScopeActivity25
            // 
            this.transactionScopeActivity25.Activities.Add(this.setStateActivity28);
            this.transactionScopeActivity25.Name = "transactionScopeActivity25";
            this.transactionScopeActivity25.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity23
            // 
            this.handleExternalEventActivity23.EventName = "Rollback";
            this.handleExternalEventActivity23.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity23.Name = "handleExternalEventActivity23";
            this.handleExternalEventActivity23.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
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
            // transactionScopeActivity23
            // 
            this.transactionScopeActivity23.Activities.Add(this.setStateActivity26);
            this.transactionScopeActivity23.Name = "transactionScopeActivity23";
            this.transactionScopeActivity23.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity21
            // 
            this.handleExternalEventActivity21.EventName = "Rollback";
            this.handleExternalEventActivity21.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity21.Name = "handleExternalEventActivity21";
            this.handleExternalEventActivity21.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
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
            // transactionScopeActivity24
            // 
            this.transactionScopeActivity24.Activities.Add(this.setStateActivity27);
            this.transactionScopeActivity24.Name = "transactionScopeActivity24";
            this.transactionScopeActivity24.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity22
            // 
            this.handleExternalEventActivity22.EventName = "Rollback";
            this.handleExternalEventActivity22.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity22.Name = "handleExternalEventActivity22";
            this.handleExternalEventActivity22.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
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
            this.transactionScopeActivity3.Activities.Add(this.ifElseActivity1);
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
            // transactionScopeActivity22
            // 
            this.transactionScopeActivity22.Activities.Add(this.setStateActivity25);
            this.transactionScopeActivity22.Name = "transactionScopeActivity22";
            this.transactionScopeActivity22.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity20
            // 
            this.handleExternalEventActivity20.EventName = "Rollback";
            this.handleExternalEventActivity20.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity20.Name = "handleExternalEventActivity20";
            this.handleExternalEventActivity20.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.rollbackEventFired1_Invoked);
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
            this.handleExternalEventActivity1.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.StartProcessingEventInvoked);
            // 
            // DraftInitCode
            // 
            this.DraftInitCode.Name = "DraftInitCode";
            this.DraftInitCode.ExecuteCode += new System.EventHandler(this.DraftInitCode_ExecuteCode);
            // 
            // handleExternalEventActivity27
            // 
            this.handleExternalEventActivity27.EventName = "SetInternalParameters";
            this.handleExternalEventActivity27.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowSupportService);
            this.handleExternalEventActivity27.Name = "handleExternalEventActivity27";
            this.handleExternalEventActivity27.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.setInternalParametersInvoked);
            // 
            // eventDrivenActivity16
            // 
            this.eventDrivenActivity16.Activities.Add(this.handleExternalEventActivity26);
            this.eventDrivenActivity16.Activities.Add(this.transactionScopeActivity28);
            this.eventDrivenActivity16.Name = "eventDrivenActivity16";
            // 
            // eventDrivenActivity15
            // 
            this.eventDrivenActivity15.Activities.Add(this.handleExternalEventActivity25);
            this.eventDrivenActivity15.Activities.Add(this.transactionScopeActivity27);
            this.eventDrivenActivity15.Name = "eventDrivenActivity15";
            // 
            // RollbackRequestedInit
            // 
            this.RollbackRequestedInit.Activities.Add(this.RollbackRequestedInitCode);
            this.RollbackRequestedInit.Name = "RollbackRequestedInit";
            // 
            // eventDrivenActivity7
            // 
            this.eventDrivenActivity7.Activities.Add(this.handleExternalEventActivity17);
            this.eventDrivenActivity7.Activities.Add(this.transactionScopeActivity19);
            this.eventDrivenActivity7.Name = "eventDrivenActivity7";
            // 
            // eventDrivenActivity4
            // 
            this.eventDrivenActivity4.Activities.Add(this.handleExternalEventActivity14);
            this.eventDrivenActivity4.Activities.Add(this.transactionScopeActivity16);
            this.eventDrivenActivity4.Name = "eventDrivenActivity4";
            // 
            // eventDrivenActivity1
            // 
            this.eventDrivenActivity1.Activities.Add(this.handleExternalEventActivity9);
            this.eventDrivenActivity1.Activities.Add(this.transactionScopeActivity12);
            this.eventDrivenActivity1.Name = "eventDrivenActivity1";
            // 
            // AgreementOPExpertSightingInit
            // 
            this.AgreementOPExpertSightingInit.Activities.Add(this.AgreementOPExpertSightingInitCode);
            this.AgreementOPExpertSightingInit.Name = "AgreementOPExpertSightingInit";
            // 
            // AgreedInit
            // 
            this.AgreedInit.Activities.Add(this.AgreedInitCode);
            this.AgreedInit.Activities.Add(this.transactionScopeActivity6);
            this.AgreedInit.Name = "AgreedInit";
            // 
            // eventDrivenActivity14
            // 
            this.eventDrivenActivity14.Activities.Add(this.handleExternalEventActivity24);
            this.eventDrivenActivity14.Activities.Add(this.transactionScopeActivity26);
            this.eventDrivenActivity14.Name = "eventDrivenActivity14";
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
            // eventDrivenActivity13
            // 
            this.eventDrivenActivity13.Activities.Add(this.handleExternalEventActivity23);
            this.eventDrivenActivity13.Activities.Add(this.transactionScopeActivity25);
            this.eventDrivenActivity13.Name = "eventDrivenActivity13";
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
            // eventDrivenActivity11
            // 
            this.eventDrivenActivity11.Activities.Add(this.handleExternalEventActivity21);
            this.eventDrivenActivity11.Activities.Add(this.transactionScopeActivity23);
            this.eventDrivenActivity11.Name = "eventDrivenActivity11";
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
            // eventDrivenActivity12
            // 
            this.eventDrivenActivity12.Activities.Add(this.handleExternalEventActivity22);
            this.eventDrivenActivity12.Activities.Add(this.transactionScopeActivity24);
            this.eventDrivenActivity12.Name = "eventDrivenActivity12";
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
            // eventDrivenActivity10
            // 
            this.eventDrivenActivity10.Activities.Add(this.handleExternalEventActivity20);
            this.eventDrivenActivity10.Activities.Add(this.transactionScopeActivity22);
            this.eventDrivenActivity10.Name = "eventDrivenActivity10";
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
            // eventDrivenActivity17
            // 
            this.eventDrivenActivity17.Activities.Add(this.handleExternalEventActivity27);
            this.eventDrivenActivity17.Name = "eventDrivenActivity17";
            // 
            // RollbackRequested
            // 
            this.RollbackRequested.Activities.Add(this.RollbackRequestedInit);
            this.RollbackRequested.Activities.Add(this.eventDrivenActivity15);
            this.RollbackRequested.Activities.Add(this.eventDrivenActivity16);
            this.RollbackRequested.Name = "RollbackRequested";
            // 
            // AgreementOPExpertSighting
            // 
            this.AgreementOPExpertSighting.Activities.Add(this.AgreementOPExpertSightingInit);
            this.AgreementOPExpertSighting.Activities.Add(this.eventDrivenActivity1);
            this.AgreementOPExpertSighting.Activities.Add(this.eventDrivenActivity4);
            this.AgreementOPExpertSighting.Activities.Add(this.eventDrivenActivity7);
            this.AgreementOPExpertSighting.Name = "AgreementOPExpertSighting";
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
            this.UPKZHeadSighting.Activities.Add(this.eventDrivenActivity14);
            this.UPKZHeadSighting.Name = "UPKZHeadSighting";
            // 
            // UPKZCuratorSighting
            // 
            this.UPKZCuratorSighting.Activities.Add(this.UPKZCuratorSightingInit);
            this.UPKZCuratorSighting.Activities.Add(this.SightingEvent4);
            this.UPKZCuratorSighting.Activities.Add(this.DenialEvent4);
            this.UPKZCuratorSighting.Activities.Add(this.eventDrivenActivity13);
            this.UPKZCuratorSighting.Name = "UPKZCuratorSighting";
            // 
            // InitiatorHeadSighting
            // 
            this.InitiatorHeadSighting.Activities.Add(this.InitiatorHeadSightingInit);
            this.InitiatorHeadSighting.Activities.Add(this.SightingEvent3);
            this.InitiatorHeadSighting.Activities.Add(this.DenialEvent3);
            this.InitiatorHeadSighting.Activities.Add(this.eventDrivenActivity11);
            this.InitiatorHeadSighting.Name = "InitiatorHeadSighting";
            // 
            // OPHeadSighting
            // 
            this.OPHeadSighting.Activities.Add(this.OPHeadSightingInit);
            this.OPHeadSighting.Activities.Add(this.SightingEvent2);
            this.OPHeadSighting.Activities.Add(this.DenialEvent2);
            this.OPHeadSighting.Activities.Add(this.eventDrivenActivity12);
            this.OPHeadSighting.Name = "OPHeadSighting";
            // 
            // OPExpertSighting
            // 
            this.OPExpertSighting.Activities.Add(this.OPExpertSightingInit);
            this.OPExpertSighting.Activities.Add(this.SightingEvent1);
            this.OPExpertSighting.Activities.Add(this.DenialEvent1);
            this.OPExpertSighting.Activities.Add(this.eventDrivenActivity10);
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
            this.Activities.Add(this.AgreementOPExpertSighting);
            this.Activities.Add(this.RollbackRequested);
            this.Activities.Add(this.eventDrivenActivity17);
            this.Comment = "";
            this.CompletedStateName = "Archived";
            this.DynamicUpdateCondition = null;
            this.InitialStateName = "Draft";
            this.Name = "Demand";
            this.CanModifyActivities = false;

        }

        #endregion

        private SetStateActivity setStateActivity7;

        private IfElseBranchActivity ifElseBranchActivity2;

        private IfElseBranchActivity ifElseBranchActivity1;

        private SetStateActivity setStateActivity11;

        private IfElseActivity ifElseActivity1;

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

        private SetStateActivity setStateActivity19;

        private TransactionScopeActivity transactionScopeActivity16;

        private HandleExternalEventActivity handleExternalEventActivity14;

        private TransactionScopeActivity transactionScopeActivity12;

        private HandleExternalEventActivity handleExternalEventActivity9;

        private CodeActivity AgreementOPExpertSightingInitCode;

        private EventDrivenActivity eventDrivenActivity4;

        private EventDrivenActivity eventDrivenActivity1;

        private StateInitializationActivity AgreementOPExpertSightingInit;

        private StateActivity AgreementOPExpertSighting;

        private StateInitializationActivity RollbackRequestedInit;

        private StateActivity RollbackRequested;

        private HandleExternalEventActivity handleExternalEventActivity17;

        private EventDrivenActivity eventDrivenActivity7;

        private SetStateActivity setStateActivity22;

        private SetStateActivity setStateActivity29;

        private SetStateActivity setStateActivity28;

        private SetStateActivity setStateActivity26;

        private SetStateActivity setStateActivity27;

        private SetStateActivity setStateActivity25;

        private CodeActivity RollbackRequestedInitCode;

        private TransactionScopeActivity transactionScopeActivity19;

        private TransactionScopeActivity transactionScopeActivity26;

        private HandleExternalEventActivity handleExternalEventActivity24;

        private TransactionScopeActivity transactionScopeActivity25;

        private HandleExternalEventActivity handleExternalEventActivity23;

        private TransactionScopeActivity transactionScopeActivity23;

        private HandleExternalEventActivity handleExternalEventActivity21;

        private TransactionScopeActivity transactionScopeActivity24;

        private HandleExternalEventActivity handleExternalEventActivity22;

        private TransactionScopeActivity transactionScopeActivity22;

        private HandleExternalEventActivity handleExternalEventActivity20;

        private EventDrivenActivity eventDrivenActivity14;

        private EventDrivenActivity eventDrivenActivity13;

        private EventDrivenActivity eventDrivenActivity11;

        private EventDrivenActivity eventDrivenActivity12;

        private EventDrivenActivity eventDrivenActivity10;

        private IfElseBranchActivity ifElseBranchActivity8;

        private IfElseActivity ifElseActivity4;

        private SetStateActivity setStateActivity30;

        private TransactionScopeActivity transactionScopeActivity28;

        private HandleExternalEventActivity handleExternalEventActivity26;

        private TransactionScopeActivity transactionScopeActivity27;

        private HandleExternalEventActivity handleExternalEventActivity25;

        private EventDrivenActivity eventDrivenActivity16;

        private EventDrivenActivity eventDrivenActivity15;

        private IfElseBranchActivity ifElseBranchActivity14;

        private IfElseBranchActivity ifElseBranchActivity13;

        private IfElseBranchActivity ifElseBranchActivity12;

        private IfElseBranchActivity ifElseBranchActivity11;

        private IfElseBranchActivity ifElseBranchActivity10;

        private SetStateActivity setStateActivity38;

        private SetStateActivity setStateActivity37;

        private SetStateActivity setStateActivity36;

        private SetStateActivity setStateActivity35;

        private SetStateActivity setStateActivity34;

        private SetStateActivity setStateActivity31;

        private HandleExternalEventActivity handleExternalEventActivity27;

        private EventDrivenActivity eventDrivenActivity17;

        private StateActivity Draft;























































    }
}
