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
    partial class DemandAdjustment
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
            System.Workflow.Activities.CodeCondition codecondition2 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition3 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition4 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition5 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition6 = new System.Workflow.Activities.CodeCondition();
            this.setStateActivity4 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity2 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity5 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity21 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity1 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity9 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity20 = new System.Workflow.Activities.SetStateActivity();
            this.UPKZCuratorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity3 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.SourceDemandLimitManagerSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity17 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.SourceDemandLimitManagerSightingInitCode1 = new System.Workflow.Activities.CodeActivity();
            this.TargetDemandLimitManagerSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity4 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.TargetDemandLimitManagerSightingInitCode1 = new System.Workflow.Activities.CodeActivity();
            this.TargetDemandLimitExecutorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity18 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.TargetDemandLimitExecutorSightingInitCode1 = new System.Workflow.Activities.CodeActivity();
            this.ifElseBranchActivity4 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity3 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity7 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity7 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity17 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity15 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity18 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity19 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity6 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity5 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity10 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity6 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity14 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity13 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity11 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity10 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity9 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity8 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity16 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity12 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity8 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity3 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity2 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity6 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.AgreedInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity13 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity10 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity12 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity9 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity16 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity14 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity15 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity13 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.ifElseActivity3 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity10 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity8 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity5 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity5 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.faultHandlersActivity1 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.ifElseActivity1 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity11 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity7 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity7 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity6 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.ifElseActivity5 = new System.Workflow.Activities.IfElseActivity();
            this.ifElseActivity4 = new System.Workflow.Activities.IfElseActivity();
            this.transactionScopeActivity14 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity12 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity9 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity11 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity8 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.denialEventFired1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity2 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity2 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.SourceDemandLimitExecutorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity1 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.DraftInitCode = new System.Workflow.Activities.CodeActivity();
            this.AgreedInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent6 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent6 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent7 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent7 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZCuratorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.SourceDemandLimitManagerSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.TargetDemandLimitManagerSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.TargetDemandLimitExecutorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.SourceDemandLimitExecutorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.StartProcessingEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.DraftInit = new System.Workflow.Activities.StateInitializationActivity();
            this.Archived = new System.Workflow.Activities.StateActivity();
            this.Agreed = new System.Workflow.Activities.StateActivity();
            this.UPKZHeadSighting = new System.Workflow.Activities.StateActivity();
            this.UPKZCuratorSighting = new System.Workflow.Activities.StateActivity();
            this.SourceDemandLimitManagerSighting = new System.Workflow.Activities.StateActivity();
            this.TargetDemandLimitManagerSighting = new System.Workflow.Activities.StateActivity();
            this.TargetDemandLimitExecutorSighting = new System.Workflow.Activities.StateActivity();
            this.SourceDemandLimitExecutorSighting = new System.Workflow.Activities.StateActivity();
            this.Draft = new System.Workflow.Activities.StateActivity();
            // 
            // setStateActivity4
            // 
            this.setStateActivity4.Name = "setStateActivity4";
            this.setStateActivity4.TargetStateName = "Agreed";
            // 
            // setStateActivity2
            // 
            this.setStateActivity2.Name = "setStateActivity2";
            this.setStateActivity2.TargetStateName = "TargetDemandLimitExecutorSighting";
            // 
            // setStateActivity5
            // 
            this.setStateActivity5.Name = "setStateActivity5";
            this.setStateActivity5.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity21
            // 
            this.setStateActivity21.Name = "setStateActivity21";
            this.setStateActivity21.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity1
            // 
            this.setStateActivity1.Name = "setStateActivity1";
            this.setStateActivity1.TargetStateName = "SourceDemandLimitExecutorSighting";
            // 
            // setStateActivity9
            // 
            this.setStateActivity9.Name = "setStateActivity9";
            this.setStateActivity9.TargetStateName = "UPKZHeadSighting";
            // 
            // setStateActivity20
            // 
            this.setStateActivity20.Name = "setStateActivity20";
            this.setStateActivity20.TargetStateName = "Agreed";
            // 
            // UPKZCuratorSightingInitCode
            // 
            this.UPKZCuratorSightingInitCode.Name = "UPKZCuratorSightingInitCode";
            this.UPKZCuratorSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZCuratorSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity3
            // 
            this.transactionScopeActivity3.Activities.Add(this.setStateActivity4);
            this.transactionScopeActivity3.Name = "transactionScopeActivity3";
            this.transactionScopeActivity3.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // SourceDemandLimitManagerSightingInitCode
            // 
            this.SourceDemandLimitManagerSightingInitCode.Name = "SourceDemandLimitManagerSightingInitCode";
            this.SourceDemandLimitManagerSightingInitCode.ExecuteCode += new System.EventHandler(this.SourceDemandLimitManagerSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity17
            // 
            this.transactionScopeActivity17.Activities.Add(this.setStateActivity2);
            this.transactionScopeActivity17.Name = "transactionScopeActivity17";
            this.transactionScopeActivity17.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // SourceDemandLimitManagerSightingInitCode1
            // 
            this.SourceDemandLimitManagerSightingInitCode1.Name = "SourceDemandLimitManagerSightingInitCode1";
            this.SourceDemandLimitManagerSightingInitCode1.ExecuteCode += new System.EventHandler(this.SourceDemandLimitManagerSightingInitCode_ExecuteCode);
            // 
            // TargetDemandLimitManagerSightingInitCode
            // 
            this.TargetDemandLimitManagerSightingInitCode.Name = "TargetDemandLimitManagerSightingInitCode";
            this.TargetDemandLimitManagerSightingInitCode.ExecuteCode += new System.EventHandler(this.TargetDemandLimitManagerSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity4
            // 
            this.transactionScopeActivity4.Activities.Add(this.setStateActivity5);
            this.transactionScopeActivity4.Name = "transactionScopeActivity4";
            this.transactionScopeActivity4.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // TargetDemandLimitManagerSightingInitCode1
            // 
            this.TargetDemandLimitManagerSightingInitCode1.Name = "TargetDemandLimitManagerSightingInitCode1";
            this.TargetDemandLimitManagerSightingInitCode1.ExecuteCode += new System.EventHandler(this.TargetDemandLimitManagerSightingInitCode_ExecuteCode);
            // 
            // TargetDemandLimitExecutorSightingInitCode
            // 
            this.TargetDemandLimitExecutorSightingInitCode.Name = "TargetDemandLimitExecutorSightingInitCode";
            this.TargetDemandLimitExecutorSightingInitCode.ExecuteCode += new System.EventHandler(this.TargetDemandLimitExecutorSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity18
            // 
            this.transactionScopeActivity18.Activities.Add(this.setStateActivity21);
            this.transactionScopeActivity18.Name = "transactionScopeActivity18";
            this.transactionScopeActivity18.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // TargetDemandLimitExecutorSightingInitCode1
            // 
            this.TargetDemandLimitExecutorSightingInitCode1.Name = "TargetDemandLimitExecutorSightingInitCode1";
            this.TargetDemandLimitExecutorSightingInitCode1.ExecuteCode += new System.EventHandler(this.TargetDemandLimitExecutorSightingInitCode1_ExecuteCode);
            // 
            // ifElseBranchActivity4
            // 
            this.ifElseBranchActivity4.Activities.Add(this.setStateActivity1);
            this.ifElseBranchActivity4.Name = "ifElseBranchActivity4";
            // 
            // ifElseBranchActivity3
            // 
            this.ifElseBranchActivity3.Activities.Add(this.setStateActivity9);
            codecondition1.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckLimitIsContingency_ExecuteCode);
            this.ifElseBranchActivity3.Condition = codecondition1;
            this.ifElseBranchActivity3.Name = "ifElseBranchActivity3";
            // 
            // ifElseBranchActivity7
            // 
            this.ifElseBranchActivity7.Activities.Add(this.setStateActivity20);
            codecondition2.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.Check_DemanAdjustmentTypeIsNotRedistrubution);
            this.ifElseBranchActivity7.Condition = codecondition2;
            this.ifElseBranchActivity7.Name = "ifElseBranchActivity7";
            // 
            // setStateActivity7
            // 
            this.setStateActivity7.Name = "setStateActivity7";
            this.setStateActivity7.TargetStateName = "Archived";
            // 
            // setStateActivity17
            // 
            this.setStateActivity17.Name = "setStateActivity17";
            this.setStateActivity17.TargetStateName = "Draft";
            // 
            // setStateActivity15
            // 
            this.setStateActivity15.Name = "setStateActivity15";
            this.setStateActivity15.TargetStateName = "Agreed";
            // 
            // setStateActivity18
            // 
            this.setStateActivity18.Name = "setStateActivity18";
            this.setStateActivity18.TargetStateName = "Draft";
            // 
            // setStateActivity19
            // 
            this.setStateActivity19.Name = "setStateActivity19";
            this.setStateActivity19.TargetStateName = "Agreed";
            // 
            // ifElseBranchActivity6
            // 
            this.ifElseBranchActivity6.Activities.Add(this.UPKZCuratorSightingInitCode);
            this.ifElseBranchActivity6.Name = "ifElseBranchActivity6";
            // 
            // ifElseBranchActivity5
            // 
            this.ifElseBranchActivity5.Activities.Add(this.transactionScopeActivity3);
            codecondition3.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckLimitDistributionIsFromFutureQuoterToCurrent_ExecuteCode);
            this.ifElseBranchActivity5.Condition = codecondition3;
            this.ifElseBranchActivity5.Name = "ifElseBranchActivity5";
            // 
            // setStateActivity10
            // 
            this.setStateActivity10.Name = "setStateActivity10";
            this.setStateActivity10.TargetStateName = "Draft";
            // 
            // setStateActivity6
            // 
            this.setStateActivity6.Name = "setStateActivity6";
            this.setStateActivity6.TargetStateName = "TargetDemandLimitExecutorSighting";
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.SourceDemandLimitManagerSightingInitCode);
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.SourceDemandLimitManagerSightingInitCode1);
            this.ifElseBranchActivity1.Activities.Add(this.transactionScopeActivity17);
            codecondition4.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckToSkipSourceDemandLimitManagerSighting_ExecuteCode);
            this.ifElseBranchActivity1.Condition = codecondition4;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // setStateActivity14
            // 
            this.setStateActivity14.Name = "setStateActivity14";
            this.setStateActivity14.TargetStateName = "Draft";
            // 
            // setStateActivity13
            // 
            this.setStateActivity13.Name = "setStateActivity13";
            this.setStateActivity13.TargetStateName = "UPKZCuratorSighting";
            // 
            // ifElseBranchActivity11
            // 
            this.ifElseBranchActivity11.Activities.Add(this.TargetDemandLimitManagerSightingInitCode);
            this.ifElseBranchActivity11.Name = "ifElseBranchActivity11";
            // 
            // ifElseBranchActivity10
            // 
            this.ifElseBranchActivity10.Activities.Add(this.TargetDemandLimitManagerSightingInitCode1);
            this.ifElseBranchActivity10.Activities.Add(this.transactionScopeActivity4);
            codecondition5.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckToSkipTargetDemandLimitManagerSighting_ExecuteCode);
            this.ifElseBranchActivity10.Condition = codecondition5;
            this.ifElseBranchActivity10.Name = "ifElseBranchActivity10";
            // 
            // ifElseBranchActivity9
            // 
            this.ifElseBranchActivity9.Activities.Add(this.TargetDemandLimitExecutorSightingInitCode);
            this.ifElseBranchActivity9.Name = "ifElseBranchActivity9";
            // 
            // ifElseBranchActivity8
            // 
            this.ifElseBranchActivity8.Activities.Add(this.TargetDemandLimitExecutorSightingInitCode1);
            this.ifElseBranchActivity8.Activities.Add(this.transactionScopeActivity18);
            codecondition6.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckToSkipTargetDemandLimitExecutorSighting_ExecuteCode);
            this.ifElseBranchActivity8.Condition = codecondition6;
            this.ifElseBranchActivity8.Name = "ifElseBranchActivity8";
            // 
            // setStateActivity16
            // 
            this.setStateActivity16.Name = "setStateActivity16";
            this.setStateActivity16.TargetStateName = "Draft";
            // 
            // setStateActivity12
            // 
            this.setStateActivity12.Name = "setStateActivity12";
            this.setStateActivity12.TargetStateName = "TargetDemandLimitManagerSighting";
            // 
            // setStateActivity8
            // 
            this.setStateActivity8.Name = "setStateActivity8";
            this.setStateActivity8.TargetStateName = "Draft";
            // 
            // setStateActivity3
            // 
            this.setStateActivity3.Name = "setStateActivity3";
            this.setStateActivity3.TargetStateName = "SourceDemandLimitManagerSighting";
            // 
            // ifElseActivity2
            // 
            this.ifElseActivity2.Activities.Add(this.ifElseBranchActivity7);
            this.ifElseActivity2.Activities.Add(this.ifElseBranchActivity3);
            this.ifElseActivity2.Activities.Add(this.ifElseBranchActivity4);
            this.ifElseActivity2.Name = "ifElseActivity2";
            // 
            // transactionScopeActivity6
            // 
            this.transactionScopeActivity6.Activities.Add(this.setStateActivity7);
            this.transactionScopeActivity6.Name = "transactionScopeActivity6";
            this.transactionScopeActivity6.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // AgreedInitCode
            // 
            this.AgreedInitCode.Name = "AgreedInitCode";
            this.AgreedInitCode.ExecuteCode += new System.EventHandler(this.AgreedInitCode_ExecuteCode);
            // 
            // transactionScopeActivity13
            // 
            this.transactionScopeActivity13.Activities.Add(this.setStateActivity17);
            this.transactionScopeActivity13.Name = "transactionScopeActivity13";
            this.transactionScopeActivity13.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity10
            // 
            this.handleExternalEventActivity10.EventName = "Denial";
            this.handleExternalEventActivity10.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity10.Name = "handleExternalEventActivity10";
            this.handleExternalEventActivity10.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity12
            // 
            this.transactionScopeActivity12.Activities.Add(this.setStateActivity15);
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
            // UPKZHeadSightingInitCode
            // 
            this.UPKZHeadSightingInitCode.Name = "UPKZHeadSightingInitCode";
            this.UPKZHeadSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZHeadSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity16
            // 
            this.transactionScopeActivity16.Activities.Add(this.setStateActivity18);
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
            // transactionScopeActivity15
            // 
            this.transactionScopeActivity15.Activities.Add(this.setStateActivity19);
            this.transactionScopeActivity15.Name = "transactionScopeActivity15";
            this.transactionScopeActivity15.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity13
            // 
            this.handleExternalEventActivity13.EventName = "Sighting";
            this.handleExternalEventActivity13.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity13.Name = "handleExternalEventActivity13";
            this.handleExternalEventActivity13.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // ifElseActivity3
            // 
            this.ifElseActivity3.Activities.Add(this.ifElseBranchActivity5);
            this.ifElseActivity3.Activities.Add(this.ifElseBranchActivity6);
            this.ifElseActivity3.Name = "ifElseActivity3";
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
            // faultHandlersActivity1
            // 
            this.faultHandlersActivity1.Name = "faultHandlersActivity1";
            // 
            // ifElseActivity1
            // 
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity1);
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity2);
            this.ifElseActivity1.Name = "ifElseActivity1";
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
            // ifElseActivity5
            // 
            this.ifElseActivity5.Activities.Add(this.ifElseBranchActivity10);
            this.ifElseActivity5.Activities.Add(this.ifElseBranchActivity11);
            this.ifElseActivity5.Name = "ifElseActivity5";
            // 
            // ifElseActivity4
            // 
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity8);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity9);
            this.ifElseActivity4.Name = "ifElseActivity4";
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
            this.transactionScopeActivity2.Activities.Add(this.setStateActivity3);
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
            // SourceDemandLimitExecutorSightingInitCode
            // 
            this.SourceDemandLimitExecutorSightingInitCode.Name = "SourceDemandLimitExecutorSightingInitCode";
            this.SourceDemandLimitExecutorSightingInitCode.ExecuteCode += new System.EventHandler(this.SourceDemandLimitExecutorSightingInitCode_ExecuteCode);
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
            // DenialEvent6
            // 
            this.DenialEvent6.Activities.Add(this.handleExternalEventActivity10);
            this.DenialEvent6.Activities.Add(this.transactionScopeActivity13);
            this.DenialEvent6.Name = "DenialEvent6";
            // 
            // SightingEvent6
            // 
            this.SightingEvent6.Activities.Add(this.handleExternalEventActivity9);
            this.SightingEvent6.Activities.Add(this.transactionScopeActivity12);
            this.SightingEvent6.Name = "SightingEvent6";
            // 
            // UPKZHeadSightingInit
            // 
            this.UPKZHeadSightingInit.Activities.Add(this.UPKZHeadSightingInitCode);
            this.UPKZHeadSightingInit.Name = "UPKZHeadSightingInit";
            // 
            // DenialEvent7
            // 
            this.DenialEvent7.Activities.Add(this.handleExternalEventActivity14);
            this.DenialEvent7.Activities.Add(this.transactionScopeActivity16);
            this.DenialEvent7.Name = "DenialEvent7";
            // 
            // SightingEvent7
            // 
            this.SightingEvent7.Activities.Add(this.handleExternalEventActivity13);
            this.SightingEvent7.Activities.Add(this.transactionScopeActivity15);
            this.SightingEvent7.Name = "SightingEvent7";
            // 
            // UPKZCuratorSightingInit
            // 
            this.UPKZCuratorSightingInit.Activities.Add(this.ifElseActivity3);
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
            // SourceDemandLimitManagerSightingInit
            // 
            this.SourceDemandLimitManagerSightingInit.Activities.Add(this.ifElseActivity1);
            this.SourceDemandLimitManagerSightingInit.Activities.Add(this.faultHandlersActivity1);
            this.SourceDemandLimitManagerSightingInit.Name = "SourceDemandLimitManagerSightingInit";
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
            // TargetDemandLimitManagerSightingInit
            // 
            this.TargetDemandLimitManagerSightingInit.Activities.Add(this.ifElseActivity5);
            this.TargetDemandLimitManagerSightingInit.Name = "TargetDemandLimitManagerSightingInit";
            // 
            // TargetDemandLimitExecutorSightingInit
            // 
            this.TargetDemandLimitExecutorSightingInit.Activities.Add(this.ifElseActivity4);
            this.TargetDemandLimitExecutorSightingInit.Name = "TargetDemandLimitExecutorSightingInit";
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
            // SourceDemandLimitExecutorSightingInit
            // 
            this.SourceDemandLimitExecutorSightingInit.Activities.Add(this.SourceDemandLimitExecutorSightingInitCode);
            this.SourceDemandLimitExecutorSightingInit.Name = "SourceDemandLimitExecutorSightingInit";
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
            this.UPKZHeadSighting.Activities.Add(this.SightingEvent6);
            this.UPKZHeadSighting.Activities.Add(this.DenialEvent6);
            this.UPKZHeadSighting.Name = "UPKZHeadSighting";
            // 
            // UPKZCuratorSighting
            // 
            this.UPKZCuratorSighting.Activities.Add(this.UPKZCuratorSightingInit);
            this.UPKZCuratorSighting.Activities.Add(this.SightingEvent7);
            this.UPKZCuratorSighting.Activities.Add(this.DenialEvent7);
            this.UPKZCuratorSighting.Name = "UPKZCuratorSighting";
            // 
            // SourceDemandLimitManagerSighting
            // 
            this.SourceDemandLimitManagerSighting.Activities.Add(this.SourceDemandLimitManagerSightingInit);
            this.SourceDemandLimitManagerSighting.Activities.Add(this.SightingEvent3);
            this.SourceDemandLimitManagerSighting.Activities.Add(this.DenialEvent3);
            this.SourceDemandLimitManagerSighting.Name = "SourceDemandLimitManagerSighting";
            // 
            // TargetDemandLimitManagerSighting
            // 
            this.TargetDemandLimitManagerSighting.Activities.Add(this.TargetDemandLimitManagerSightingInit);
            this.TargetDemandLimitManagerSighting.Activities.Add(this.SightingEvent5);
            this.TargetDemandLimitManagerSighting.Activities.Add(this.DenialEvent5);
            this.TargetDemandLimitManagerSighting.Name = "TargetDemandLimitManagerSighting";
            // 
            // TargetDemandLimitExecutorSighting
            // 
            this.TargetDemandLimitExecutorSighting.Activities.Add(this.SightingEvent4);
            this.TargetDemandLimitExecutorSighting.Activities.Add(this.DenialEvent4);
            this.TargetDemandLimitExecutorSighting.Activities.Add(this.TargetDemandLimitExecutorSightingInit);
            this.TargetDemandLimitExecutorSighting.Name = "TargetDemandLimitExecutorSighting";
            // 
            // SourceDemandLimitExecutorSighting
            // 
            this.SourceDemandLimitExecutorSighting.Activities.Add(this.SourceDemandLimitExecutorSightingInit);
            this.SourceDemandLimitExecutorSighting.Activities.Add(this.SightingEvent1);
            this.SourceDemandLimitExecutorSighting.Activities.Add(this.DenialEvent1);
            this.SourceDemandLimitExecutorSighting.Name = "SourceDemandLimitExecutorSighting";
            // 
            // Draft
            // 
            this.Draft.Activities.Add(this.DraftInit);
            this.Draft.Activities.Add(this.StartProcessingEvent);
            this.Draft.Name = "Draft";
            // 
            // DemandAdjustment
            // 
            this.Activities.Add(this.Draft);
            this.Activities.Add(this.SourceDemandLimitExecutorSighting);
            this.Activities.Add(this.TargetDemandLimitExecutorSighting);
            this.Activities.Add(this.TargetDemandLimitManagerSighting);
            this.Activities.Add(this.SourceDemandLimitManagerSighting);
            this.Activities.Add(this.UPKZCuratorSighting);
            this.Activities.Add(this.UPKZHeadSighting);
            this.Activities.Add(this.Agreed);
            this.Activities.Add(this.Archived);
            this.Comment = "";
            this.CompletedStateName = "Archived";
            this.DynamicUpdateCondition = null;
            this.InitialStateName = "Draft";
            this.Name = "DemandAdjustment";
            this.CanModifyActivities = false;

        }

        #endregion

        private StateActivity SourceDemandLimitExecutorSighting;

        private StateActivity UPKZHeadSighting;

        private StateActivity UPKZCuratorSighting;

        private StateActivity SourceDemandLimitManagerSighting;

        private StateActivity TargetDemandLimitManagerSighting;

        private StateActivity TargetDemandLimitExecutorSighting;

        private StateActivity Archived;

        private StateActivity Agreed;

        private TransactionScopeActivity transactionScopeActivity1;

        private HandleExternalEventActivity handleExternalEventActivity1;

        private CodeActivity DraftInitCode;

        private EventDrivenActivity StartProcessingEvent;

        private StateInitializationActivity DraftInit;

        private SetStateActivity setStateActivity1;

        private SetStateActivity setStateActivity8;

        private TransactionScopeActivity transactionScopeActivity8;

        private HandleExternalEventActivity denialEventFired1;

        private TransactionScopeActivity transactionScopeActivity2;

        private HandleExternalEventActivity handleExternalEventActivity2;

        private CodeActivity SourceDemandLimitExecutorSightingInitCode;

        private EventDrivenActivity DenialEvent1;

        private EventDrivenActivity SightingEvent1;

        private StateInitializationActivity SourceDemandLimitExecutorSightingInit;

        private SetStateActivity setStateActivity10;

        private TransactionScopeActivity transactionScopeActivity10;

        private HandleExternalEventActivity handleExternalEventActivity8;

        private TransactionScopeActivity transactionScopeActivity5;

        private HandleExternalEventActivity handleExternalEventActivity5;

        private EventDrivenActivity DenialEvent3;

        private EventDrivenActivity SightingEvent3;

        private SetStateActivity setStateActivity7;

        private SetStateActivity setStateActivity6;

        private TransactionScopeActivity transactionScopeActivity6;

        private StateInitializationActivity AgreedInit;

        private SetStateActivity setStateActivity9;

        private IfElseBranchActivity ifElseBranchActivity4;

        private IfElseBranchActivity ifElseBranchActivity3;

        private IfElseActivity ifElseActivity2;

        private SetStateActivity setStateActivity16;

        private SetStateActivity setStateActivity12;

        private TransactionScopeActivity transactionScopeActivity14;

        private HandleExternalEventActivity handleExternalEventActivity12;

        private TransactionScopeActivity transactionScopeActivity9;

        private HandleExternalEventActivity handleExternalEventActivity11;

        private StateInitializationActivity TargetDemandLimitExecutorSightingInit;

        private EventDrivenActivity DenialEvent4;

        private EventDrivenActivity SightingEvent4;

        private SetStateActivity setStateActivity14;

        private SetStateActivity setStateActivity13;

        private TransactionScopeActivity transactionScopeActivity11;

        private HandleExternalEventActivity handleExternalEventActivity7;

        private TransactionScopeActivity transactionScopeActivity7;

        private HandleExternalEventActivity handleExternalEventActivity6;

        private CodeActivity TargetDemandLimitManagerSightingInitCode;

        private EventDrivenActivity DenialEvent5;

        private EventDrivenActivity SightingEvent5;

        private StateInitializationActivity TargetDemandLimitManagerSightingInit;

        private SetStateActivity setStateActivity17;

        private SetStateActivity setStateActivity15;

        private TransactionScopeActivity transactionScopeActivity13;

        private HandleExternalEventActivity handleExternalEventActivity10;

        private TransactionScopeActivity transactionScopeActivity12;

        private HandleExternalEventActivity handleExternalEventActivity9;

        private CodeActivity UPKZHeadSightingInitCode;

        private EventDrivenActivity DenialEvent6;

        private EventDrivenActivity SightingEvent6;

        private StateInitializationActivity UPKZHeadSightingInit;

        private SetStateActivity setStateActivity18;

        private TransactionScopeActivity transactionScopeActivity16;

        private HandleExternalEventActivity handleExternalEventActivity14;

        private TransactionScopeActivity transactionScopeActivity15;

        private HandleExternalEventActivity handleExternalEventActivity13;

        private CodeActivity UPKZCuratorSightingInitCode;

        private EventDrivenActivity DenialEvent7;

        private EventDrivenActivity SightingEvent7;

        private StateInitializationActivity UPKZCuratorSightingInit;

        private SetStateActivity setStateActivity19;

        private SetStateActivity setStateActivity20;

        private IfElseBranchActivity ifElseBranchActivity7;

        private SetStateActivity setStateActivity4;

        private SetStateActivity setStateActivity5;

        private SetStateActivity setStateActivity21;

        private TransactionScopeActivity transactionScopeActivity3;

        private TransactionScopeActivity transactionScopeActivity4;

        private TransactionScopeActivity transactionScopeActivity18;

        private IfElseBranchActivity ifElseBranchActivity6;

        private IfElseBranchActivity ifElseBranchActivity5;

        private IfElseBranchActivity ifElseBranchActivity11;

        private IfElseBranchActivity ifElseBranchActivity10;

        private IfElseBranchActivity ifElseBranchActivity9;

        private IfElseBranchActivity ifElseBranchActivity8;

        private IfElseActivity ifElseActivity3;

        private IfElseActivity ifElseActivity5;

        private IfElseActivity ifElseActivity4;

        private SetStateActivity setStateActivity2;

        private TransactionScopeActivity transactionScopeActivity17;

        private IfElseBranchActivity ifElseBranchActivity2;

        private IfElseBranchActivity ifElseBranchActivity1;

        private SetStateActivity setStateActivity3;

        private CodeActivity AgreedInitCode;

        private FaultHandlersActivity faultHandlersActivity1;

        private IfElseActivity ifElseActivity1;

        private CodeActivity SourceDemandLimitManagerSightingInitCode;

        private CodeActivity TargetDemandLimitExecutorSightingInitCode;

        private StateInitializationActivity SourceDemandLimitManagerSightingInit;

        private CodeActivity SourceDemandLimitManagerSightingInitCode1;

        private CodeActivity TargetDemandLimitManagerSightingInitCode1;

        private CodeActivity TargetDemandLimitExecutorSightingInitCode1;

        private StateActivity Draft;

































































































































    }
}
