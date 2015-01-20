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
    partial class BillDemand
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
            System.Workflow.Activities.CodeCondition codecondition3 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition4 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition5 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition6 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition7 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition8 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition9 = new System.Workflow.Activities.CodeCondition();
            System.Workflow.Activities.CodeCondition codecondition10 = new System.Workflow.Activities.CodeCondition();
            this.setStateActivity41 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity40 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity39 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity38 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity37 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity36 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity32 = new System.Workflow.Activities.SetStateActivity();
            this.WriteCommentToHistoryCode = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity33 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity31 = new System.Workflow.Activities.SetStateActivity();
            this.SetExternalParameters = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity13 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity46 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity18 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity9 = new System.Workflow.Activities.SetStateActivity();
            this.ResetLimitExecutorSightsCode1 = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity47 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity5 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity21 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity7 = new System.Workflow.Activities.SetStateActivity();
            this.ResetLimitManagerSights1 = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity3 = new System.Workflow.Activities.SetStateActivity();
            this.ResetLimitManagerSightsCode1 = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity2 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity27 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity21 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity20 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity19 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity18 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity17 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity16 = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity35 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseBranchActivity15 = new System.Workflow.Activities.IfElseBranchActivity();
            this.faultHandlersActivity2 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.ifElseBranchActivity12 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity11 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity1 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity7 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity3 = new System.Workflow.Activities.IfElseBranchActivity();
            this.CheckLimitExecutorSight = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity42 = new System.Workflow.Activities.SetStateActivity();
            this.SetTransferDateCode2 = new System.Workflow.Activities.CodeActivity();
            this.ifElseBranchActivity22 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity6 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity5 = new System.Workflow.Activities.IfElseBranchActivity();
            this.transactionScopeActivity29 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.ifElseBranchActivity4 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseBranchActivity10 = new System.Workflow.Activities.IfElseBranchActivity();
            this.CheckInitiatorHeadMustSign = new System.Workflow.Activities.IfElseBranchActivity();
            this.setStateActivity28 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity45 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity44 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity23 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity22 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity12 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity8 = new System.Workflow.Activities.IfElseActivity();
            this.PrepareForRoute1 = new System.Workflow.Activities.CodeActivity();
            this.faultHandlerActivity1 = new System.Workflow.ComponentModel.FaultHandlerActivity();
            this.ifElseActivity6 = new System.Workflow.Activities.IfElseActivity();
            this.CheckExternalStatus = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity29 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity26 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity25 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity24 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity1 = new System.Workflow.Activities.IfElseActivity();
            this.HeadInitiatorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.setStateActivity20 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity19 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity9 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity17 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity16 = new System.Workflow.Activities.SetStateActivity();
            this.ResetLimitExecutorSightsCode2 = new System.Workflow.Activities.CodeActivity();
            this.ifElseActivity2 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity15 = new System.Workflow.Activities.SetStateActivity();
            this.faultHandlersActivity1 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.transactionScopeActivity7 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.setStateActivity11 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity10 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity4 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity6 = new System.Workflow.Activities.SetStateActivity();
            this.setStateActivity4 = new System.Workflow.Activities.SetStateActivity();
            this.ResetLimitManagerSightsCode2 = new System.Workflow.Activities.CodeActivity();
            this.ifElseBranchActivity2 = new System.Workflow.Activities.IfElseBranchActivity();
            this.ifElseActivity3 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity8 = new System.Workflow.Activities.SetStateActivity();
            this.ifElseActivity5 = new System.Workflow.Activities.IfElseActivity();
            this.setStateActivity1 = new System.Workflow.Activities.SetStateActivity();
            this.PrepareForRouteCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity28 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity27 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.InitiatorConfirmationInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity31 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity28 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity30 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity25 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity27 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity24 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.faultHandlersActivity4 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.transactionScopeActivity26 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.ExportBillDemandCode = new System.Workflow.Activities.CodeActivity();
            this.handleExternalEventActivity23 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.InAccountingWithExportInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity24 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity22 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity23 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity21 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.DraftForTechnicalDenialInitCode = new System.Workflow.Activities.CodeActivity();
            this.faultHandlersActivity3 = new System.Workflow.ComponentModel.FaultHandlersActivity();
            this.transactionScopeActivity12 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.transactionScopeActivity22 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.PaidInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity21 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity19 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity20 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity18 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity19 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity17 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity25 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.UPKZHeadSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity18 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity16 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity17 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity15 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity16 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity14 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.LimitExecutorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity15 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity13 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity14 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity12 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity9 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity11 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.setStateActivity30 = new System.Workflow.Activities.SetStateActivity();
            this.handleExternalEventActivity20 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.setStateActivity14 = new System.Workflow.Activities.SetStateActivity();
            this.delayActivity1 = new System.Workflow.Activities.DelayActivity();
            this.OnPaymentInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity13 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity10 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.PostingAccountingInitCode = new System.Workflow.Activities.CodeActivity();
            this.sequenceActivity1 = new System.Workflow.Activities.SequenceActivity();
            this.handleExternalEventActivity7 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity11 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity9 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity10 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity8 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZCuratorSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity5 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity5 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity6 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity6 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity4 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity4 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.ifElseActivity7 = new System.Workflow.Activities.IfElseActivity();
            this.LimitManagerSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity3 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity3 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.transactionScopeActivity8 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.denialEventFired1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.UPKZCntrollerSightingInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity2 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity2 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.DraftInitCode = new System.Workflow.Activities.CodeActivity();
            this.transactionScopeActivity1 = new System.Workflow.ComponentModel.TransactionScopeActivity();
            this.handleExternalEventActivity1 = new System.Workflow.Activities.HandleExternalEventActivity();
            this.PaidEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.InitiatorConfirmationInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent11 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent9 = new System.Workflow.Activities.EventDrivenActivity();
            this.PaidEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.ExportEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.InAccountingWithExportInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent8 = new System.Workflow.Activities.EventDrivenActivity();
            this.StartProcessingEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.DraftForTechnicalDenialInit = new System.Workflow.Activities.StateInitializationActivity();
            this.OnPaymentCheckStatusInit = new System.Workflow.Activities.StateInitializationActivity();
            this.PaidInit = new System.Workflow.Activities.StateInitializationActivity();
            this.SightingEvent7 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent7 = new System.Workflow.Activities.EventDrivenActivity();
            this.TechnicalDenialEvent7 = new System.Workflow.Activities.EventDrivenActivity();
            this.HeadInitiatorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.UPKZHeadSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.TechnicalDenialEvent6 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent6 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent6 = new System.Workflow.Activities.EventDrivenActivity();
            this.LimitExecutorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.TechnicalDenialEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.SightingEvent4 = new System.Workflow.Activities.EventDrivenActivity();
            this.CheckStatusManually = new System.Workflow.Activities.EventDrivenActivity();
            this.CheckStatusOnTimer = new System.Workflow.Activities.EventDrivenActivity();
            this.OnPaymentInit = new System.Workflow.Activities.StateInitializationActivity();
            this.DenialEvent5 = new System.Workflow.Activities.EventDrivenActivity();
            this.PostingAccountingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.PostingAccountingEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.TechnicalDenialEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZCuratorSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.SightingEvent3 = new System.Workflow.Activities.EventDrivenActivity();
            this.TechnicalDenialEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.LimitManagerSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.SightingEvent2 = new System.Workflow.Activities.EventDrivenActivity();
            this.DenialEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.UPKZCntrollerSightingInit = new System.Workflow.Activities.StateInitializationActivity();
            this.SightingEvent1 = new System.Workflow.Activities.EventDrivenActivity();
            this.DraftInit = new System.Workflow.Activities.StateInitializationActivity();
            this.StartProcessingEvent = new System.Workflow.Activities.EventDrivenActivity();
            this.InitiatorConfirmation = new System.Workflow.Activities.StateActivity();
            this.InAccountingWithExport = new System.Workflow.Activities.StateActivity();
            this.DraftForTechnicalDenial = new System.Workflow.Activities.StateActivity();
            this.OnPaymentCheckStatus = new System.Workflow.Activities.StateActivity();
            this.Paid = new System.Workflow.Activities.StateActivity();
            this.HeadInitiatorSighting = new System.Workflow.Activities.StateActivity();
            this.UPKZHeadSighting = new System.Workflow.Activities.StateActivity();
            this.LimitExecutorSighting = new System.Workflow.Activities.StateActivity();
            this.OnPayment = new System.Workflow.Activities.StateActivity();
            this.Archived = new System.Workflow.Activities.StateActivity();
            this.PostingAccounting = new System.Workflow.Activities.StateActivity();
            this.UPKZCuratorSighting = new System.Workflow.Activities.StateActivity();
            this.LimitManagerSighting = new System.Workflow.Activities.StateActivity();
            this.UPKZCntrollerSighting = new System.Workflow.Activities.StateActivity();
            this.Draft = new System.Workflow.Activities.StateActivity();
            // 
            // setStateActivity41
            // 
            this.setStateActivity41.Name = "setStateActivity41";
            this.setStateActivity41.TargetStateName = "HeadInitiatorSighting";
            // 
            // setStateActivity40
            // 
            this.setStateActivity40.Name = "setStateActivity40";
            this.setStateActivity40.TargetStateName = "UPKZCntrollerSighting";
            // 
            // setStateActivity39
            // 
            this.setStateActivity39.Name = "setStateActivity39";
            this.setStateActivity39.TargetStateName = "UPKZHeadSighting";
            // 
            // setStateActivity38
            // 
            this.setStateActivity38.Name = "setStateActivity38";
            this.setStateActivity38.TargetStateName = "LimitExecutorSighting";
            // 
            // setStateActivity37
            // 
            this.setStateActivity37.Name = "setStateActivity37";
            this.setStateActivity37.TargetStateName = "LimitManagerSighting";
            // 
            // setStateActivity36
            // 
            this.setStateActivity36.Name = "setStateActivity36";
            this.setStateActivity36.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity32
            // 
            this.setStateActivity32.Name = "setStateActivity32";
            this.setStateActivity32.TargetStateName = "OnPayment";
            // 
            // WriteCommentToHistoryCode
            // 
            this.WriteCommentToHistoryCode.Name = "WriteCommentToHistoryCode";
            this.WriteCommentToHistoryCode.ExecuteCode += new System.EventHandler(this.WriteCommentToHistoryCode_ExecuteCode);
            // 
            // setStateActivity33
            // 
            this.setStateActivity33.Name = "setStateActivity33";
            this.setStateActivity33.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity31
            // 
            this.setStateActivity31.Name = "setStateActivity31";
            this.setStateActivity31.TargetStateName = "Paid";
            // 
            // SetExternalParameters
            // 
            this.SetExternalParameters.Name = "SetExternalParameters";
            this.SetExternalParameters.ExecuteCode += new System.EventHandler(this.SetExternalParameters_ExecuteCode);
            // 
            // setStateActivity13
            // 
            this.setStateActivity13.Name = "setStateActivity13";
            this.setStateActivity13.TargetStateName = "LimitExecutorSighting";
            // 
            // setStateActivity46
            // 
            this.setStateActivity46.Name = "setStateActivity46";
            this.setStateActivity46.TargetStateName = "InitiatorConfirmation";
            // 
            // setStateActivity18
            // 
            this.setStateActivity18.Name = "setStateActivity18";
            this.setStateActivity18.TargetStateName = "PostingAccounting";
            // 
            // setStateActivity9
            // 
            this.setStateActivity9.Name = "setStateActivity9";
            this.setStateActivity9.TargetStateName = "LimitManagerSighting";
            // 
            // ResetLimitExecutorSightsCode1
            // 
            this.ResetLimitExecutorSightsCode1.Name = "ResetLimitExecutorSightsCode1";
            this.ResetLimitExecutorSightsCode1.ExecuteCode += new System.EventHandler(this.ResetLimitExecutorSights_ExecuteCode);
            // 
            // setStateActivity47
            // 
            this.setStateActivity47.Name = "setStateActivity47";
            this.setStateActivity47.TargetStateName = "InitiatorConfirmation";
            // 
            // setStateActivity5
            // 
            this.setStateActivity5.Name = "setStateActivity5";
            this.setStateActivity5.TargetStateName = "PostingAccounting";
            // 
            // setStateActivity21
            // 
            this.setStateActivity21.Name = "setStateActivity21";
            this.setStateActivity21.TargetStateName = "UPKZHeadSighting";
            // 
            // setStateActivity7
            // 
            this.setStateActivity7.Name = "setStateActivity7";
            this.setStateActivity7.TargetStateName = "UPKZCuratorSighting";
            // 
            // ResetLimitManagerSights1
            // 
            this.ResetLimitManagerSights1.Name = "ResetLimitManagerSights1";
            this.ResetLimitManagerSights1.ExecuteCode += new System.EventHandler(this.ResetLimitManagerSights_ExecuteCode);
            // 
            // setStateActivity3
            // 
            this.setStateActivity3.Name = "setStateActivity3";
            this.setStateActivity3.TargetStateName = "UPKZCuratorSighting";
            // 
            // ResetLimitManagerSightsCode1
            // 
            this.ResetLimitManagerSightsCode1.Name = "ResetLimitManagerSightsCode1";
            this.ResetLimitManagerSightsCode1.ExecuteCode += new System.EventHandler(this.ResetLimitManagerSights_ExecuteCode);
            // 
            // setStateActivity2
            // 
            this.setStateActivity2.Name = "setStateActivity2";
            this.setStateActivity2.TargetStateName = "LimitExecutorSighting";
            // 
            // setStateActivity27
            // 
            this.setStateActivity27.Name = "setStateActivity27";
            this.setStateActivity27.TargetStateName = "HeadInitiatorSighting";
            // 
            // ifElseBranchActivity21
            // 
            this.ifElseBranchActivity21.Activities.Add(this.setStateActivity41);
            ruleconditionreference1.ConditionName = "Condition7";
            this.ifElseBranchActivity21.Condition = ruleconditionreference1;
            this.ifElseBranchActivity21.Name = "ifElseBranchActivity21";
            // 
            // ifElseBranchActivity20
            // 
            this.ifElseBranchActivity20.Activities.Add(this.setStateActivity40);
            ruleconditionreference2.ConditionName = "Condition6";
            this.ifElseBranchActivity20.Condition = ruleconditionreference2;
            this.ifElseBranchActivity20.Name = "ifElseBranchActivity20";
            // 
            // ifElseBranchActivity19
            // 
            this.ifElseBranchActivity19.Activities.Add(this.setStateActivity39);
            ruleconditionreference3.ConditionName = "Condition5";
            this.ifElseBranchActivity19.Condition = ruleconditionreference3;
            this.ifElseBranchActivity19.Name = "ifElseBranchActivity19";
            // 
            // ifElseBranchActivity18
            // 
            this.ifElseBranchActivity18.Activities.Add(this.setStateActivity38);
            ruleconditionreference4.ConditionName = "Condition4";
            this.ifElseBranchActivity18.Condition = ruleconditionreference4;
            this.ifElseBranchActivity18.Name = "ifElseBranchActivity18";
            // 
            // ifElseBranchActivity17
            // 
            this.ifElseBranchActivity17.Activities.Add(this.setStateActivity37);
            ruleconditionreference5.ConditionName = "Condition3";
            this.ifElseBranchActivity17.Condition = ruleconditionreference5;
            this.ifElseBranchActivity17.Name = "ifElseBranchActivity17";
            // 
            // ifElseBranchActivity16
            // 
            this.ifElseBranchActivity16.Activities.Add(this.setStateActivity36);
            ruleconditionreference6.ConditionName = "Condition2";
            this.ifElseBranchActivity16.Condition = ruleconditionreference6;
            this.ifElseBranchActivity16.Name = "ifElseBranchActivity16";
            // 
            // setStateActivity35
            // 
            this.setStateActivity35.Name = "setStateActivity35";
            this.setStateActivity35.TargetStateName = "OnPayment";
            // 
            // ifElseBranchActivity15
            // 
            this.ifElseBranchActivity15.Activities.Add(this.WriteCommentToHistoryCode);
            this.ifElseBranchActivity15.Activities.Add(this.setStateActivity32);
            this.ifElseBranchActivity15.Name = "ifElseBranchActivity15";
            // 
            // faultHandlersActivity2
            // 
            this.faultHandlersActivity2.Name = "faultHandlersActivity2";
            // 
            // ifElseBranchActivity12
            // 
            this.ifElseBranchActivity12.Activities.Add(this.setStateActivity33);
            codecondition1.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckIsRejected_ExecuteCode);
            this.ifElseBranchActivity12.Condition = codecondition1;
            this.ifElseBranchActivity12.Name = "ifElseBranchActivity12";
            // 
            // ifElseBranchActivity11
            // 
            this.ifElseBranchActivity11.Activities.Add(this.SetExternalParameters);
            this.ifElseBranchActivity11.Activities.Add(this.setStateActivity31);
            codecondition2.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckIsPaid_ExecuteCode);
            this.ifElseBranchActivity11.Condition = codecondition2;
            this.ifElseBranchActivity11.Name = "ifElseBranchActivity11";
            // 
            // ifElseBranchActivity1
            // 
            this.ifElseBranchActivity1.Activities.Add(this.setStateActivity13);
            codecondition3.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckBillDemandInitiatorHeadIsAutoSight_ExecuteCode);
            this.ifElseBranchActivity1.Condition = codecondition3;
            this.ifElseBranchActivity1.Name = "ifElseBranchActivity1";
            // 
            // ifElseBranchActivity7
            // 
            this.ifElseBranchActivity7.Activities.Add(this.setStateActivity46);
            this.ifElseBranchActivity7.Name = "ifElseBranchActivity7";
            // 
            // ifElseBranchActivity3
            // 
            this.ifElseBranchActivity3.Activities.Add(this.setStateActivity18);
            codecondition4.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckBillDemandFilialIsSupportedExport_ExecuteCode);
            this.ifElseBranchActivity3.Condition = codecondition4;
            this.ifElseBranchActivity3.Name = "ifElseBranchActivity3";
            // 
            // CheckLimitExecutorSight
            // 
            this.CheckLimitExecutorSight.Activities.Add(this.ResetLimitExecutorSightsCode1);
            this.CheckLimitExecutorSight.Activities.Add(this.setStateActivity9);
            codecondition5.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckLimitExecutorSight_ExecuteCode);
            this.CheckLimitExecutorSight.Condition = codecondition5;
            this.CheckLimitExecutorSight.Name = "CheckLimitExecutorSight";
            // 
            // setStateActivity42
            // 
            this.setStateActivity42.Name = "setStateActivity42";
            this.setStateActivity42.TargetStateName = "InAccountingWithExport";
            // 
            // SetTransferDateCode2
            // 
            this.SetTransferDateCode2.Name = "SetTransferDateCode2";
            this.SetTransferDateCode2.ExecuteCode += new System.EventHandler(this.SetTransferDateCode_ExecuteCode);
            // 
            // ifElseBranchActivity22
            // 
            this.ifElseBranchActivity22.Activities.Add(this.setStateActivity47);
            this.ifElseBranchActivity22.Name = "ifElseBranchActivity22";
            // 
            // ifElseBranchActivity6
            // 
            this.ifElseBranchActivity6.Activities.Add(this.setStateActivity5);
            codecondition6.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckBillDemandFilialIsSupportedExport_ExecuteCode);
            this.ifElseBranchActivity6.Condition = codecondition6;
            this.ifElseBranchActivity6.Name = "ifElseBranchActivity6";
            // 
            // ifElseBranchActivity5
            // 
            this.ifElseBranchActivity5.Activities.Add(this.setStateActivity21);
            codecondition7.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckBillDemandValue_ExecuteCode);
            this.ifElseBranchActivity5.Condition = codecondition7;
            this.ifElseBranchActivity5.Name = "ifElseBranchActivity5";
            // 
            // transactionScopeActivity29
            // 
            this.transactionScopeActivity29.Activities.Add(this.ResetLimitManagerSights1);
            this.transactionScopeActivity29.Activities.Add(this.setStateActivity7);
            this.transactionScopeActivity29.Name = "transactionScopeActivity29";
            this.transactionScopeActivity29.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // ifElseBranchActivity4
            // 
            this.ifElseBranchActivity4.Activities.Add(this.ResetLimitManagerSightsCode1);
            this.ifElseBranchActivity4.Activities.Add(this.setStateActivity3);
            codecondition8.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckLimitMangerSight_ExecuteCode);
            this.ifElseBranchActivity4.Condition = codecondition8;
            this.ifElseBranchActivity4.Name = "ifElseBranchActivity4";
            // 
            // ifElseBranchActivity10
            // 
            this.ifElseBranchActivity10.Activities.Add(this.setStateActivity2);
            this.ifElseBranchActivity10.Name = "ifElseBranchActivity10";
            // 
            // CheckInitiatorHeadMustSign
            // 
            this.CheckInitiatorHeadMustSign.Activities.Add(this.setStateActivity27);
            codecondition9.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckInitiatorHeadMustSign_ExecuteCode);
            this.CheckInitiatorHeadMustSign.Condition = codecondition9;
            this.CheckInitiatorHeadMustSign.Name = "CheckInitiatorHeadMustSign";
            // 
            // setStateActivity28
            // 
            this.setStateActivity28.Name = "setStateActivity28";
            this.setStateActivity28.TargetStateName = "Paid";
            // 
            // setStateActivity45
            // 
            this.setStateActivity45.Name = "setStateActivity45";
            this.setStateActivity45.TargetStateName = "Draft";
            // 
            // setStateActivity44
            // 
            this.setStateActivity44.Name = "setStateActivity44";
            this.setStateActivity44.TargetStateName = "UPKZCuratorSighting";
            // 
            // setStateActivity23
            // 
            this.setStateActivity23.Name = "setStateActivity23";
            this.setStateActivity23.TargetStateName = "Paid";
            // 
            // setStateActivity22
            // 
            this.setStateActivity22.Name = "setStateActivity22";
            this.setStateActivity22.TargetStateName = "OnPayment";
            // 
            // setStateActivity12
            // 
            this.setStateActivity12.Name = "setStateActivity12";
            this.setStateActivity12.TargetStateName = "Draft";
            // 
            // ifElseActivity8
            // 
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity16);
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity17);
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity18);
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity19);
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity20);
            this.ifElseActivity8.Activities.Add(this.ifElseBranchActivity21);
            this.ifElseActivity8.Name = "ifElseActivity8";
            // 
            // PrepareForRoute1
            // 
            this.PrepareForRoute1.Name = "PrepareForRoute1";
            this.PrepareForRoute1.ExecuteCode += new System.EventHandler(this.PrepareForRoute_ExecuteCode);
            // 
            // faultHandlerActivity1
            // 
            this.faultHandlerActivity1.Activities.Add(this.setStateActivity35);
            this.faultHandlerActivity1.FaultType = typeof(System.Exception);
            this.faultHandlerActivity1.Name = "faultHandlerActivity1";
            // 
            // ifElseActivity6
            // 
            this.ifElseActivity6.Activities.Add(this.ifElseBranchActivity11);
            this.ifElseActivity6.Activities.Add(this.ifElseBranchActivity12);
            this.ifElseActivity6.Activities.Add(this.faultHandlersActivity2);
            this.ifElseActivity6.Activities.Add(this.ifElseBranchActivity15);
            this.ifElseActivity6.Name = "ifElseActivity6";
            // 
            // CheckExternalStatus
            // 
            this.CheckExternalStatus.Name = "CheckExternalStatus";
            this.CheckExternalStatus.ExecuteCode += new System.EventHandler(this.CheckExternalStatus_ExecuteCode);
            // 
            // setStateActivity29
            // 
            this.setStateActivity29.Name = "setStateActivity29";
            this.setStateActivity29.TargetStateName = "Archived";
            // 
            // setStateActivity26
            // 
            this.setStateActivity26.Name = "setStateActivity26";
            this.setStateActivity26.TargetStateName = "LimitExecutorSighting";
            // 
            // setStateActivity25
            // 
            this.setStateActivity25.Name = "setStateActivity25";
            this.setStateActivity25.TargetStateName = "Draft";
            // 
            // setStateActivity24
            // 
            this.setStateActivity24.Name = "setStateActivity24";
            this.setStateActivity24.TargetStateName = "DraftForTechnicalDenial";
            // 
            // ifElseActivity1
            // 
            this.ifElseActivity1.Activities.Add(this.ifElseBranchActivity1);
            this.ifElseActivity1.Name = "ifElseActivity1";
            // 
            // HeadInitiatorSightingInitCode
            // 
            this.HeadInitiatorSightingInitCode.Name = "HeadInitiatorSightingInitCode";
            this.HeadInitiatorSightingInitCode.ExecuteCode += new System.EventHandler(this.HeadInitiatorSightingInitCode_ExecuteCode);
            // 
            // setStateActivity20
            // 
            this.setStateActivity20.Name = "setStateActivity20";
            this.setStateActivity20.TargetStateName = "DraftForTechnicalDenial";
            // 
            // setStateActivity19
            // 
            this.setStateActivity19.Name = "setStateActivity19";
            this.setStateActivity19.TargetStateName = "Draft";
            // 
            // ifElseActivity9
            // 
            this.ifElseActivity9.Activities.Add(this.ifElseBranchActivity3);
            this.ifElseActivity9.Activities.Add(this.ifElseBranchActivity7);
            this.ifElseActivity9.Name = "ifElseActivity9";
            // 
            // setStateActivity17
            // 
            this.setStateActivity17.Name = "setStateActivity17";
            this.setStateActivity17.TargetStateName = "DraftForTechnicalDenial";
            // 
            // setStateActivity16
            // 
            this.setStateActivity16.Name = "setStateActivity16";
            this.setStateActivity16.TargetStateName = "Draft";
            // 
            // ResetLimitExecutorSightsCode2
            // 
            this.ResetLimitExecutorSightsCode2.Name = "ResetLimitExecutorSightsCode2";
            this.ResetLimitExecutorSightsCode2.ExecuteCode += new System.EventHandler(this.ResetLimitExecutorSights_ExecuteCode);
            // 
            // ifElseActivity2
            // 
            this.ifElseActivity2.Activities.Add(this.CheckLimitExecutorSight);
            this.ifElseActivity2.Name = "ifElseActivity2";
            // 
            // setStateActivity15
            // 
            this.setStateActivity15.Name = "setStateActivity15";
            this.setStateActivity15.TargetStateName = "Draft";
            // 
            // faultHandlersActivity1
            // 
            this.faultHandlersActivity1.Name = "faultHandlersActivity1";
            // 
            // transactionScopeActivity7
            // 
            this.transactionScopeActivity7.Activities.Add(this.SetTransferDateCode2);
            this.transactionScopeActivity7.Activities.Add(this.setStateActivity42);
            this.transactionScopeActivity7.Name = "transactionScopeActivity7";
            this.transactionScopeActivity7.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // setStateActivity11
            // 
            this.setStateActivity11.Name = "setStateActivity11";
            this.setStateActivity11.TargetStateName = "DraftForTechnicalDenial";
            // 
            // setStateActivity10
            // 
            this.setStateActivity10.Name = "setStateActivity10";
            this.setStateActivity10.TargetStateName = "Draft";
            // 
            // ifElseActivity4
            // 
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity5);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity6);
            this.ifElseActivity4.Activities.Add(this.ifElseBranchActivity22);
            this.ifElseActivity4.Name = "ifElseActivity4";
            // 
            // setStateActivity6
            // 
            this.setStateActivity6.Name = "setStateActivity6";
            this.setStateActivity6.TargetStateName = "DraftForTechnicalDenial";
            // 
            // setStateActivity4
            // 
            this.setStateActivity4.Name = "setStateActivity4";
            this.setStateActivity4.TargetStateName = "Draft";
            // 
            // ResetLimitManagerSightsCode2
            // 
            this.ResetLimitManagerSightsCode2.Name = "ResetLimitManagerSightsCode2";
            this.ResetLimitManagerSightsCode2.ExecuteCode += new System.EventHandler(this.ResetLimitManagerSights_ExecuteCode);
            // 
            // ifElseBranchActivity2
            // 
            this.ifElseBranchActivity2.Activities.Add(this.transactionScopeActivity29);
            codecondition10.Condition += new System.EventHandler<System.Workflow.Activities.ConditionalEventArgs>(this.CheckLimitManagerSightingComplete_ExecuteCode);
            this.ifElseBranchActivity2.Condition = codecondition10;
            this.ifElseBranchActivity2.Name = "ifElseBranchActivity2";
            // 
            // ifElseActivity3
            // 
            this.ifElseActivity3.Activities.Add(this.ifElseBranchActivity4);
            this.ifElseActivity3.Name = "ifElseActivity3";
            // 
            // setStateActivity8
            // 
            this.setStateActivity8.Name = "setStateActivity8";
            this.setStateActivity8.TargetStateName = "Draft";
            // 
            // ifElseActivity5
            // 
            this.ifElseActivity5.Activities.Add(this.CheckInitiatorHeadMustSign);
            this.ifElseActivity5.Activities.Add(this.ifElseBranchActivity10);
            this.ifElseActivity5.Name = "ifElseActivity5";
            // 
            // setStateActivity1
            // 
            this.setStateActivity1.Name = "setStateActivity1";
            this.setStateActivity1.TargetStateName = "UPKZCntrollerSighting";
            // 
            // PrepareForRouteCode
            // 
            this.PrepareForRouteCode.Name = "PrepareForRouteCode";
            this.PrepareForRouteCode.ExecuteCode += new System.EventHandler(this.PrepareForRoute_ExecuteCode);
            // 
            // transactionScopeActivity28
            // 
            this.transactionScopeActivity28.Activities.Add(this.setStateActivity28);
            this.transactionScopeActivity28.Name = "transactionScopeActivity28";
            this.transactionScopeActivity28.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity27
            // 
            this.handleExternalEventActivity27.EventName = "SetPaidStatus";
            this.handleExternalEventActivity27.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity27.Name = "handleExternalEventActivity27";
            this.handleExternalEventActivity27.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.paidEventInvoked);
            // 
            // InitiatorConfirmationInitCode
            // 
            this.InitiatorConfirmationInitCode.Name = "InitiatorConfirmationInitCode";
            this.InitiatorConfirmationInitCode.ExecuteCode += new System.EventHandler(this.InitiatorConfirmationInitCode_ExecuteCode);
            // 
            // transactionScopeActivity31
            // 
            this.transactionScopeActivity31.Activities.Add(this.setStateActivity45);
            this.transactionScopeActivity31.Name = "transactionScopeActivity31";
            this.transactionScopeActivity31.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity28
            // 
            this.handleExternalEventActivity28.EventName = "Denial";
            this.handleExternalEventActivity28.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity28.Name = "handleExternalEventActivity28";
            this.handleExternalEventActivity28.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity30
            // 
            this.transactionScopeActivity30.Activities.Add(this.setStateActivity44);
            this.transactionScopeActivity30.Name = "transactionScopeActivity30";
            this.transactionScopeActivity30.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity25
            // 
            this.handleExternalEventActivity25.EventName = "SetDenialStatus";
            this.handleExternalEventActivity25.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity25.Name = "handleExternalEventActivity25";
            this.handleExternalEventActivity25.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity27
            // 
            this.transactionScopeActivity27.Activities.Add(this.setStateActivity23);
            this.transactionScopeActivity27.Name = "transactionScopeActivity27";
            this.transactionScopeActivity27.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity24
            // 
            this.handleExternalEventActivity24.EventName = "SetPaidStatus";
            this.handleExternalEventActivity24.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity24.Name = "handleExternalEventActivity24";
            this.handleExternalEventActivity24.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.paidEventInvoked);
            // 
            // faultHandlersActivity4
            // 
            this.faultHandlersActivity4.Name = "faultHandlersActivity4";
            // 
            // transactionScopeActivity26
            // 
            this.transactionScopeActivity26.Activities.Add(this.setStateActivity22);
            this.transactionScopeActivity26.Name = "transactionScopeActivity26";
            this.transactionScopeActivity26.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // ExportBillDemandCode
            // 
            this.ExportBillDemandCode.Name = "ExportBillDemandCode";
            this.ExportBillDemandCode.ExecuteCode += new System.EventHandler(this.ExportBillDemandCode_ExecuteCode);
            // 
            // handleExternalEventActivity23
            // 
            this.handleExternalEventActivity23.EventName = "Export";
            this.handleExternalEventActivity23.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity23.Name = "handleExternalEventActivity23";
            this.handleExternalEventActivity23.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.exportEventInvoked);
            // 
            // InAccountingWithExportInitCode
            // 
            this.InAccountingWithExportInitCode.Name = "InAccountingWithExportInitCode";
            this.InAccountingWithExportInitCode.ExecuteCode += new System.EventHandler(this.InAccountingWithExportInitCode_ExecuteCode);
            // 
            // transactionScopeActivity24
            // 
            this.transactionScopeActivity24.Activities.Add(this.setStateActivity12);
            this.transactionScopeActivity24.Name = "transactionScopeActivity24";
            this.transactionScopeActivity24.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity22
            // 
            this.handleExternalEventActivity22.EventName = "Denial";
            this.handleExternalEventActivity22.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity22.Name = "handleExternalEventActivity22";
            this.handleExternalEventActivity22.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity23
            // 
            this.transactionScopeActivity23.Activities.Add(this.PrepareForRoute1);
            this.transactionScopeActivity23.Activities.Add(this.ifElseActivity8);
            this.transactionScopeActivity23.Name = "transactionScopeActivity23";
            this.transactionScopeActivity23.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity21
            // 
            this.handleExternalEventActivity21.EventName = "StartProcessing";
            this.handleExternalEventActivity21.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity21.Name = "handleExternalEventActivity21";
            this.handleExternalEventActivity21.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // DraftForTechnicalDenialInitCode
            // 
            this.DraftForTechnicalDenialInitCode.Name = "DraftForTechnicalDenialInitCode";
            this.DraftForTechnicalDenialInitCode.ExecuteCode += new System.EventHandler(this.DraftForTechnicalDenialInitCode_ExecuteCode);
            // 
            // faultHandlersActivity3
            // 
            this.faultHandlersActivity3.Activities.Add(this.faultHandlerActivity1);
            this.faultHandlersActivity3.Name = "faultHandlersActivity3";
            // 
            // transactionScopeActivity12
            // 
            this.transactionScopeActivity12.Activities.Add(this.CheckExternalStatus);
            this.transactionScopeActivity12.Activities.Add(this.ifElseActivity6);
            this.transactionScopeActivity12.Name = "transactionScopeActivity12";
            this.transactionScopeActivity12.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // transactionScopeActivity22
            // 
            this.transactionScopeActivity22.Activities.Add(this.setStateActivity29);
            this.transactionScopeActivity22.Name = "transactionScopeActivity22";
            this.transactionScopeActivity22.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // PaidInitCode
            // 
            this.PaidInitCode.Name = "PaidInitCode";
            this.PaidInitCode.ExecuteCode += new System.EventHandler(this.PaidInitCode_ExecuteCode);
            // 
            // transactionScopeActivity21
            // 
            this.transactionScopeActivity21.Activities.Add(this.setStateActivity26);
            this.transactionScopeActivity21.Name = "transactionScopeActivity21";
            this.transactionScopeActivity21.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity19
            // 
            this.handleExternalEventActivity19.EventName = "Sighting";
            this.handleExternalEventActivity19.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity19.Name = "handleExternalEventActivity19";
            this.handleExternalEventActivity19.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // transactionScopeActivity20
            // 
            this.transactionScopeActivity20.Activities.Add(this.setStateActivity25);
            this.transactionScopeActivity20.Name = "transactionScopeActivity20";
            this.transactionScopeActivity20.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity18
            // 
            this.handleExternalEventActivity18.EventName = "Denial";
            this.handleExternalEventActivity18.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity18.Name = "handleExternalEventActivity18";
            this.handleExternalEventActivity18.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity19
            // 
            this.transactionScopeActivity19.Activities.Add(this.setStateActivity24);
            this.transactionScopeActivity19.Name = "transactionScopeActivity19";
            this.transactionScopeActivity19.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity17
            // 
            this.handleExternalEventActivity17.EventName = "DenialByTechnicalCauses";
            this.handleExternalEventActivity17.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity17.Name = "handleExternalEventActivity17";
            this.handleExternalEventActivity17.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.technicalDenialEventFired1_Invoked);
            // 
            // transactionScopeActivity25
            // 
            this.transactionScopeActivity25.Activities.Add(this.HeadInitiatorSightingInitCode);
            this.transactionScopeActivity25.Activities.Add(this.ifElseActivity1);
            this.transactionScopeActivity25.Name = "transactionScopeActivity25";
            this.transactionScopeActivity25.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // UPKZHeadSightingInitCode
            // 
            this.UPKZHeadSightingInitCode.Name = "UPKZHeadSightingInitCode";
            this.UPKZHeadSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZHeadSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity18
            // 
            this.transactionScopeActivity18.Activities.Add(this.setStateActivity20);
            this.transactionScopeActivity18.Name = "transactionScopeActivity18";
            this.transactionScopeActivity18.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity16
            // 
            this.handleExternalEventActivity16.EventName = "DenialByTechnicalCauses";
            this.handleExternalEventActivity16.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity16.Name = "handleExternalEventActivity16";
            this.handleExternalEventActivity16.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.technicalDenialEventFired1_Invoked);
            // 
            // transactionScopeActivity17
            // 
            this.transactionScopeActivity17.Activities.Add(this.setStateActivity19);
            this.transactionScopeActivity17.Name = "transactionScopeActivity17";
            this.transactionScopeActivity17.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity15
            // 
            this.handleExternalEventActivity15.EventName = "Denial";
            this.handleExternalEventActivity15.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity15.Name = "handleExternalEventActivity15";
            this.handleExternalEventActivity15.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.denialEventFired1_Invoked);
            // 
            // transactionScopeActivity16
            // 
            this.transactionScopeActivity16.Activities.Add(this.ifElseActivity9);
            this.transactionScopeActivity16.Name = "transactionScopeActivity16";
            this.transactionScopeActivity16.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity14
            // 
            this.handleExternalEventActivity14.EventName = "Sighting";
            this.handleExternalEventActivity14.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity14.Name = "handleExternalEventActivity14";
            this.handleExternalEventActivity14.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // LimitExecutorSightingInitCode
            // 
            this.LimitExecutorSightingInitCode.Name = "LimitExecutorSightingInitCode";
            this.LimitExecutorSightingInitCode.ExecuteCode += new System.EventHandler(this.LimitExecutorSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity15
            // 
            this.transactionScopeActivity15.Activities.Add(this.setStateActivity17);
            this.transactionScopeActivity15.Name = "transactionScopeActivity15";
            this.transactionScopeActivity15.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity13
            // 
            this.handleExternalEventActivity13.EventName = "DenialByTechnicalCauses";
            this.handleExternalEventActivity13.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity13.Name = "handleExternalEventActivity13";
            this.handleExternalEventActivity13.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.technicalDenialEventFired1_Invoked);
            // 
            // transactionScopeActivity14
            // 
            this.transactionScopeActivity14.Activities.Add(this.ResetLimitExecutorSightsCode2);
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
            this.transactionScopeActivity9.Activities.Add(this.ifElseActivity2);
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
            // setStateActivity30
            // 
            this.setStateActivity30.Name = "setStateActivity30";
            this.setStateActivity30.TargetStateName = "OnPaymentCheckStatus";
            // 
            // handleExternalEventActivity20
            // 
            this.handleExternalEventActivity20.EventName = "CheckStatus";
            this.handleExternalEventActivity20.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity20.Name = "handleExternalEventActivity20";
            // 
            // setStateActivity14
            // 
            this.setStateActivity14.Name = "setStateActivity14";
            this.setStateActivity14.TargetStateName = "OnPaymentCheckStatus";
            // 
            // delayActivity1
            // 
            this.delayActivity1.Name = "delayActivity1";
            this.delayActivity1.TimeoutDuration = System.TimeSpan.Parse("00:10:00");
            // 
            // OnPaymentInitCode
            // 
            this.OnPaymentInitCode.Name = "OnPaymentInitCode";
            this.OnPaymentInitCode.ExecuteCode += new System.EventHandler(this.OnPaymentInitCode_ExecuteCode);
            // 
            // transactionScopeActivity13
            // 
            this.transactionScopeActivity13.Activities.Add(this.setStateActivity15);
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
            // PostingAccountingInitCode
            // 
            this.PostingAccountingInitCode.Name = "PostingAccountingInitCode";
            this.PostingAccountingInitCode.ExecuteCode += new System.EventHandler(this.PostingAccountingInitCode_ExecuteCode);
            // 
            // sequenceActivity1
            // 
            this.sequenceActivity1.Activities.Add(this.transactionScopeActivity7);
            this.sequenceActivity1.Activities.Add(this.faultHandlersActivity1);
            this.sequenceActivity1.Name = "sequenceActivity1";
            // 
            // handleExternalEventActivity7
            // 
            this.handleExternalEventActivity7.EventName = "PostingAccounting";
            this.handleExternalEventActivity7.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IBillDemandWorkflowService);
            this.handleExternalEventActivity7.Name = "handleExternalEventActivity7";
            this.handleExternalEventActivity7.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.forvadTransitionEventInvoked);
            // 
            // transactionScopeActivity11
            // 
            this.transactionScopeActivity11.Activities.Add(this.setStateActivity11);
            this.transactionScopeActivity11.Name = "transactionScopeActivity11";
            this.transactionScopeActivity11.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity9
            // 
            this.handleExternalEventActivity9.EventName = "DenialByTechnicalCauses";
            this.handleExternalEventActivity9.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity9.Name = "handleExternalEventActivity9";
            this.handleExternalEventActivity9.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.technicalDenialEventFired1_Invoked);
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
            // UPKZCuratorSightingInitCode
            // 
            this.UPKZCuratorSightingInitCode.Name = "UPKZCuratorSightingInitCode";
            this.UPKZCuratorSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZCuratorSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity5
            // 
            this.transactionScopeActivity5.Activities.Add(this.ifElseActivity4);
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
            // transactionScopeActivity6
            // 
            this.transactionScopeActivity6.Activities.Add(this.setStateActivity6);
            this.transactionScopeActivity6.Name = "transactionScopeActivity6";
            this.transactionScopeActivity6.TransactionOptions.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            // 
            // handleExternalEventActivity6
            // 
            this.handleExternalEventActivity6.EventName = "DenialByTechnicalCauses";
            this.handleExternalEventActivity6.InterfaceType = typeof(Budget2.Server.Workflow.Interface.Services.IWorkflowInitService);
            this.handleExternalEventActivity6.Name = "handleExternalEventActivity6";
            this.handleExternalEventActivity6.Invoked += new System.EventHandler<System.Workflow.Activities.ExternalDataEventArgs>(this.technicalDenialEventFired1_Invoked);
            // 
            // transactionScopeActivity4
            // 
            this.transactionScopeActivity4.Activities.Add(this.ResetLimitManagerSightsCode2);
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
            // ifElseActivity7
            // 
            this.ifElseActivity7.Activities.Add(this.ifElseBranchActivity2);
            this.ifElseActivity7.Name = "ifElseActivity7";
            // 
            // LimitManagerSightingInitCode
            // 
            this.LimitManagerSightingInitCode.Name = "LimitManagerSightingInitCode";
            this.LimitManagerSightingInitCode.ExecuteCode += new System.EventHandler(this.LimitManagerSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity3
            // 
            this.transactionScopeActivity3.Activities.Add(this.ifElseActivity3);
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
            // UPKZCntrollerSightingInitCode
            // 
            this.UPKZCntrollerSightingInitCode.Name = "UPKZCntrollerSightingInitCode";
            this.UPKZCntrollerSightingInitCode.ExecuteCode += new System.EventHandler(this.UPKZCntrollerSightingInitCode_ExecuteCode);
            // 
            // transactionScopeActivity2
            // 
            this.transactionScopeActivity2.Activities.Add(this.ifElseActivity5);
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
            // DraftInitCode
            // 
            this.DraftInitCode.Name = "DraftInitCode";
            this.DraftInitCode.ExecuteCode += new System.EventHandler(this.DraftInitCode_ExecuteCode);
            // 
            // transactionScopeActivity1
            // 
            this.transactionScopeActivity1.Activities.Add(this.PrepareForRouteCode);
            this.transactionScopeActivity1.Activities.Add(this.setStateActivity1);
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
            // PaidEvent2
            // 
            this.PaidEvent2.Activities.Add(this.handleExternalEventActivity27);
            this.PaidEvent2.Activities.Add(this.transactionScopeActivity28);
            this.PaidEvent2.Name = "PaidEvent2";
            // 
            // InitiatorConfirmationInit
            // 
            this.InitiatorConfirmationInit.Activities.Add(this.InitiatorConfirmationInitCode);
            this.InitiatorConfirmationInit.Name = "InitiatorConfirmationInit";
            // 
            // DenialEvent11
            // 
            this.DenialEvent11.Activities.Add(this.handleExternalEventActivity28);
            this.DenialEvent11.Activities.Add(this.transactionScopeActivity31);
            this.DenialEvent11.Name = "DenialEvent11";
            // 
            // DenialEvent9
            // 
            this.DenialEvent9.Activities.Add(this.handleExternalEventActivity25);
            this.DenialEvent9.Activities.Add(this.transactionScopeActivity30);
            this.DenialEvent9.Name = "DenialEvent9";
            // 
            // PaidEvent1
            // 
            this.PaidEvent1.Activities.Add(this.handleExternalEventActivity24);
            this.PaidEvent1.Activities.Add(this.transactionScopeActivity27);
            this.PaidEvent1.Name = "PaidEvent1";
            // 
            // ExportEvent
            // 
            this.ExportEvent.Activities.Add(this.handleExternalEventActivity23);
            this.ExportEvent.Activities.Add(this.ExportBillDemandCode);
            this.ExportEvent.Activities.Add(this.transactionScopeActivity26);
            this.ExportEvent.Activities.Add(this.faultHandlersActivity4);
            this.ExportEvent.Name = "ExportEvent";
            // 
            // InAccountingWithExportInit
            // 
            this.InAccountingWithExportInit.Activities.Add(this.InAccountingWithExportInitCode);
            this.InAccountingWithExportInit.Name = "InAccountingWithExportInit";
            // 
            // DenialEvent8
            // 
            this.DenialEvent8.Activities.Add(this.handleExternalEventActivity22);
            this.DenialEvent8.Activities.Add(this.transactionScopeActivity24);
            this.DenialEvent8.Name = "DenialEvent8";
            // 
            // StartProcessingEvent1
            // 
            this.StartProcessingEvent1.Activities.Add(this.handleExternalEventActivity21);
            this.StartProcessingEvent1.Activities.Add(this.transactionScopeActivity23);
            this.StartProcessingEvent1.Name = "StartProcessingEvent1";
            // 
            // DraftForTechnicalDenialInit
            // 
            this.DraftForTechnicalDenialInit.Activities.Add(this.DraftForTechnicalDenialInitCode);
            this.DraftForTechnicalDenialInit.Name = "DraftForTechnicalDenialInit";
            // 
            // OnPaymentCheckStatusInit
            // 
            this.OnPaymentCheckStatusInit.Activities.Add(this.transactionScopeActivity12);
            this.OnPaymentCheckStatusInit.Activities.Add(this.faultHandlersActivity3);
            this.OnPaymentCheckStatusInit.Name = "OnPaymentCheckStatusInit";
            // 
            // PaidInit
            // 
            this.PaidInit.Activities.Add(this.PaidInitCode);
            this.PaidInit.Activities.Add(this.transactionScopeActivity22);
            this.PaidInit.Name = "PaidInit";
            // 
            // SightingEvent7
            // 
            this.SightingEvent7.Activities.Add(this.handleExternalEventActivity19);
            this.SightingEvent7.Activities.Add(this.transactionScopeActivity21);
            this.SightingEvent7.Name = "SightingEvent7";
            // 
            // DenialEvent7
            // 
            this.DenialEvent7.Activities.Add(this.handleExternalEventActivity18);
            this.DenialEvent7.Activities.Add(this.transactionScopeActivity20);
            this.DenialEvent7.Name = "DenialEvent7";
            // 
            // TechnicalDenialEvent7
            // 
            this.TechnicalDenialEvent7.Activities.Add(this.handleExternalEventActivity17);
            this.TechnicalDenialEvent7.Activities.Add(this.transactionScopeActivity19);
            this.TechnicalDenialEvent7.Name = "TechnicalDenialEvent7";
            // 
            // HeadInitiatorSightingInit
            // 
            this.HeadInitiatorSightingInit.Activities.Add(this.transactionScopeActivity25);
            this.HeadInitiatorSightingInit.Name = "HeadInitiatorSightingInit";
            // 
            // UPKZHeadSightingInit
            // 
            this.UPKZHeadSightingInit.Activities.Add(this.UPKZHeadSightingInitCode);
            this.UPKZHeadSightingInit.Name = "UPKZHeadSightingInit";
            // 
            // TechnicalDenialEvent6
            // 
            this.TechnicalDenialEvent6.Activities.Add(this.handleExternalEventActivity16);
            this.TechnicalDenialEvent6.Activities.Add(this.transactionScopeActivity18);
            this.TechnicalDenialEvent6.Name = "TechnicalDenialEvent6";
            // 
            // DenialEvent6
            // 
            this.DenialEvent6.Activities.Add(this.handleExternalEventActivity15);
            this.DenialEvent6.Activities.Add(this.transactionScopeActivity17);
            this.DenialEvent6.Name = "DenialEvent6";
            // 
            // SightingEvent6
            // 
            this.SightingEvent6.Activities.Add(this.handleExternalEventActivity14);
            this.SightingEvent6.Activities.Add(this.transactionScopeActivity16);
            this.SightingEvent6.Name = "SightingEvent6";
            // 
            // LimitExecutorSightingInit
            // 
            this.LimitExecutorSightingInit.Activities.Add(this.LimitExecutorSightingInitCode);
            this.LimitExecutorSightingInit.Name = "LimitExecutorSightingInit";
            // 
            // TechnicalDenialEvent4
            // 
            this.TechnicalDenialEvent4.Activities.Add(this.handleExternalEventActivity13);
            this.TechnicalDenialEvent4.Activities.Add(this.transactionScopeActivity15);
            this.TechnicalDenialEvent4.Name = "TechnicalDenialEvent4";
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
            // CheckStatusManually
            // 
            this.CheckStatusManually.Activities.Add(this.handleExternalEventActivity20);
            this.CheckStatusManually.Activities.Add(this.setStateActivity30);
            this.CheckStatusManually.Name = "CheckStatusManually";
            // 
            // CheckStatusOnTimer
            // 
            this.CheckStatusOnTimer.Activities.Add(this.delayActivity1);
            this.CheckStatusOnTimer.Activities.Add(this.setStateActivity14);
            this.CheckStatusOnTimer.Name = "CheckStatusOnTimer";
            // 
            // OnPaymentInit
            // 
            this.OnPaymentInit.Activities.Add(this.OnPaymentInitCode);
            this.OnPaymentInit.Name = "OnPaymentInit";
            // 
            // DenialEvent5
            // 
            this.DenialEvent5.Activities.Add(this.handleExternalEventActivity10);
            this.DenialEvent5.Activities.Add(this.transactionScopeActivity13);
            this.DenialEvent5.Name = "DenialEvent5";
            // 
            // PostingAccountingInit
            // 
            this.PostingAccountingInit.Activities.Add(this.PostingAccountingInitCode);
            this.PostingAccountingInit.Name = "PostingAccountingInit";
            // 
            // PostingAccountingEvent
            // 
            this.PostingAccountingEvent.Activities.Add(this.handleExternalEventActivity7);
            this.PostingAccountingEvent.Activities.Add(this.sequenceActivity1);
            this.PostingAccountingEvent.Name = "PostingAccountingEvent";
            // 
            // TechnicalDenialEvent3
            // 
            this.TechnicalDenialEvent3.Activities.Add(this.handleExternalEventActivity9);
            this.TechnicalDenialEvent3.Activities.Add(this.transactionScopeActivity11);
            this.TechnicalDenialEvent3.Name = "TechnicalDenialEvent3";
            // 
            // DenialEvent3
            // 
            this.DenialEvent3.Activities.Add(this.handleExternalEventActivity8);
            this.DenialEvent3.Activities.Add(this.transactionScopeActivity10);
            this.DenialEvent3.Name = "DenialEvent3";
            // 
            // UPKZCuratorSightingInit
            // 
            this.UPKZCuratorSightingInit.Activities.Add(this.UPKZCuratorSightingInitCode);
            this.UPKZCuratorSightingInit.Name = "UPKZCuratorSightingInit";
            // 
            // SightingEvent3
            // 
            this.SightingEvent3.Activities.Add(this.handleExternalEventActivity5);
            this.SightingEvent3.Activities.Add(this.transactionScopeActivity5);
            this.SightingEvent3.Name = "SightingEvent3";
            // 
            // TechnicalDenialEvent2
            // 
            this.TechnicalDenialEvent2.Activities.Add(this.handleExternalEventActivity6);
            this.TechnicalDenialEvent2.Activities.Add(this.transactionScopeActivity6);
            this.TechnicalDenialEvent2.Name = "TechnicalDenialEvent2";
            // 
            // DenialEvent2
            // 
            this.DenialEvent2.Activities.Add(this.handleExternalEventActivity4);
            this.DenialEvent2.Activities.Add(this.transactionScopeActivity4);
            this.DenialEvent2.Name = "DenialEvent2";
            // 
            // LimitManagerSightingInit
            // 
            this.LimitManagerSightingInit.Activities.Add(this.LimitManagerSightingInitCode);
            this.LimitManagerSightingInit.Activities.Add(this.ifElseActivity7);
            this.LimitManagerSightingInit.Name = "LimitManagerSightingInit";
            // 
            // SightingEvent2
            // 
            this.SightingEvent2.Activities.Add(this.handleExternalEventActivity3);
            this.SightingEvent2.Activities.Add(this.transactionScopeActivity3);
            this.SightingEvent2.Name = "SightingEvent2";
            // 
            // DenialEvent1
            // 
            this.DenialEvent1.Activities.Add(this.denialEventFired1);
            this.DenialEvent1.Activities.Add(this.transactionScopeActivity8);
            this.DenialEvent1.Name = "DenialEvent1";
            // 
            // UPKZCntrollerSightingInit
            // 
            this.UPKZCntrollerSightingInit.Activities.Add(this.UPKZCntrollerSightingInitCode);
            this.UPKZCntrollerSightingInit.Name = "UPKZCntrollerSightingInit";
            // 
            // SightingEvent1
            // 
            this.SightingEvent1.Activities.Add(this.handleExternalEventActivity2);
            this.SightingEvent1.Activities.Add(this.transactionScopeActivity2);
            this.SightingEvent1.Name = "SightingEvent1";
            // 
            // DraftInit
            // 
            this.DraftInit.Activities.Add(this.DraftInitCode);
            this.DraftInit.Name = "DraftInit";
            // 
            // StartProcessingEvent
            // 
            this.StartProcessingEvent.Activities.Add(this.handleExternalEventActivity1);
            this.StartProcessingEvent.Activities.Add(this.transactionScopeActivity1);
            this.StartProcessingEvent.Name = "StartProcessingEvent";
            // 
            // InitiatorConfirmation
            // 
            this.InitiatorConfirmation.Activities.Add(this.DenialEvent11);
            this.InitiatorConfirmation.Activities.Add(this.InitiatorConfirmationInit);
            this.InitiatorConfirmation.Activities.Add(this.PaidEvent2);
            this.InitiatorConfirmation.Name = "InitiatorConfirmation";
            // 
            // InAccountingWithExport
            // 
            this.InAccountingWithExport.Activities.Add(this.InAccountingWithExportInit);
            this.InAccountingWithExport.Activities.Add(this.ExportEvent);
            this.InAccountingWithExport.Activities.Add(this.PaidEvent1);
            this.InAccountingWithExport.Activities.Add(this.DenialEvent9);
            this.InAccountingWithExport.Name = "InAccountingWithExport";
            // 
            // DraftForTechnicalDenial
            // 
            this.DraftForTechnicalDenial.Activities.Add(this.DraftForTechnicalDenialInit);
            this.DraftForTechnicalDenial.Activities.Add(this.StartProcessingEvent1);
            this.DraftForTechnicalDenial.Activities.Add(this.DenialEvent8);
            this.DraftForTechnicalDenial.Name = "DraftForTechnicalDenial";
            // 
            // OnPaymentCheckStatus
            // 
            this.OnPaymentCheckStatus.Activities.Add(this.OnPaymentCheckStatusInit);
            this.OnPaymentCheckStatus.Name = "OnPaymentCheckStatus";
            // 
            // Paid
            // 
            this.Paid.Activities.Add(this.PaidInit);
            this.Paid.Name = "Paid";
            // 
            // HeadInitiatorSighting
            // 
            this.HeadInitiatorSighting.Activities.Add(this.HeadInitiatorSightingInit);
            this.HeadInitiatorSighting.Activities.Add(this.TechnicalDenialEvent7);
            this.HeadInitiatorSighting.Activities.Add(this.DenialEvent7);
            this.HeadInitiatorSighting.Activities.Add(this.SightingEvent7);
            this.HeadInitiatorSighting.Name = "HeadInitiatorSighting";
            // 
            // UPKZHeadSighting
            // 
            this.UPKZHeadSighting.Activities.Add(this.SightingEvent6);
            this.UPKZHeadSighting.Activities.Add(this.DenialEvent6);
            this.UPKZHeadSighting.Activities.Add(this.TechnicalDenialEvent6);
            this.UPKZHeadSighting.Activities.Add(this.UPKZHeadSightingInit);
            this.UPKZHeadSighting.Name = "UPKZHeadSighting";
            // 
            // LimitExecutorSighting
            // 
            this.LimitExecutorSighting.Activities.Add(this.SightingEvent4);
            this.LimitExecutorSighting.Activities.Add(this.DenialEvent4);
            this.LimitExecutorSighting.Activities.Add(this.TechnicalDenialEvent4);
            this.LimitExecutorSighting.Activities.Add(this.LimitExecutorSightingInit);
            this.LimitExecutorSighting.Name = "LimitExecutorSighting";
            // 
            // OnPayment
            // 
            this.OnPayment.Activities.Add(this.OnPaymentInit);
            this.OnPayment.Activities.Add(this.CheckStatusOnTimer);
            this.OnPayment.Activities.Add(this.CheckStatusManually);
            this.OnPayment.Name = "OnPayment";
            // 
            // Archived
            // 
            this.Archived.Name = "Archived";
            // 
            // PostingAccounting
            // 
            this.PostingAccounting.Activities.Add(this.PostingAccountingEvent);
            this.PostingAccounting.Activities.Add(this.PostingAccountingInit);
            this.PostingAccounting.Activities.Add(this.DenialEvent5);
            this.PostingAccounting.Name = "PostingAccounting";
            // 
            // UPKZCuratorSighting
            // 
            this.UPKZCuratorSighting.Activities.Add(this.SightingEvent3);
            this.UPKZCuratorSighting.Activities.Add(this.UPKZCuratorSightingInit);
            this.UPKZCuratorSighting.Activities.Add(this.DenialEvent3);
            this.UPKZCuratorSighting.Activities.Add(this.TechnicalDenialEvent3);
            this.UPKZCuratorSighting.Name = "UPKZCuratorSighting";
            // 
            // LimitManagerSighting
            // 
            this.LimitManagerSighting.Activities.Add(this.SightingEvent2);
            this.LimitManagerSighting.Activities.Add(this.LimitManagerSightingInit);
            this.LimitManagerSighting.Activities.Add(this.DenialEvent2);
            this.LimitManagerSighting.Activities.Add(this.TechnicalDenialEvent2);
            this.LimitManagerSighting.Name = "LimitManagerSighting";
            // 
            // UPKZCntrollerSighting
            // 
            this.UPKZCntrollerSighting.Activities.Add(this.SightingEvent1);
            this.UPKZCntrollerSighting.Activities.Add(this.UPKZCntrollerSightingInit);
            this.UPKZCntrollerSighting.Activities.Add(this.DenialEvent1);
            this.UPKZCntrollerSighting.Name = "UPKZCntrollerSighting";
            // 
            // Draft
            // 
            this.Draft.Activities.Add(this.StartProcessingEvent);
            this.Draft.Activities.Add(this.DraftInit);
            this.Draft.Name = "Draft";
            // 
            // BillDemand
            // 
            this.Activities.Add(this.Draft);
            this.Activities.Add(this.UPKZCntrollerSighting);
            this.Activities.Add(this.LimitManagerSighting);
            this.Activities.Add(this.UPKZCuratorSighting);
            this.Activities.Add(this.PostingAccounting);
            this.Activities.Add(this.Archived);
            this.Activities.Add(this.OnPayment);
            this.Activities.Add(this.LimitExecutorSighting);
            this.Activities.Add(this.UPKZHeadSighting);
            this.Activities.Add(this.HeadInitiatorSighting);
            this.Activities.Add(this.Paid);
            this.Activities.Add(this.OnPaymentCheckStatus);
            this.Activities.Add(this.DraftForTechnicalDenial);
            this.Activities.Add(this.InAccountingWithExport);
            this.Activities.Add(this.InitiatorConfirmation);
            this.Comment = "";
            this.CompletedStateName = "Archived";
            this.DynamicUpdateCondition = null;
            this.InitialStateName = "Draft";
            this.Name = "BillDemand";
            this.CanModifyActivities = false;

        }

        #endregion

        private SetStateActivity setStateActivity7;

        private CodeActivity ResetLimitManagerSights1;

        private TransactionScopeActivity transactionScopeActivity29;

        private IfElseBranchActivity ifElseBranchActivity2;

        private IfElseActivity ifElseActivity7;

        private FaultHandlersActivity faultHandlersActivity4;

        private SetStateActivity setStateActivity45;

        private TransactionScopeActivity transactionScopeActivity31;

        private HandleExternalEventActivity handleExternalEventActivity28;

        private StateInitializationActivity InitiatorConfirmationInit;

        private EventDrivenActivity DenialEvent11;

        private StateActivity InitiatorConfirmation;

        private SetStateActivity setStateActivity46;

        private SetStateActivity setStateActivity47;

        private IfElseBranchActivity ifElseBranchActivity7;

        private IfElseBranchActivity ifElseBranchActivity3;

        private IfElseBranchActivity ifElseBranchActivity22;

        private IfElseActivity ifElseActivity9;

        private CodeActivity InitiatorConfirmationInitCode;

        private CodeActivity SetTransferDateCode2;

        private SetStateActivity setStateActivity44;

        private TransactionScopeActivity transactionScopeActivity30;

        private HandleExternalEventActivity handleExternalEventActivity23;

        private EventDrivenActivity ExportEvent;

        private SetStateActivity setStateActivity22;

        private CodeActivity ExportBillDemandCode;

        private TransactionScopeActivity transactionScopeActivity26;

        private SetStateActivity setStateActivity23;

        private TransactionScopeActivity transactionScopeActivity27;

        private HandleExternalEventActivity handleExternalEventActivity24;

        private EventDrivenActivity PaidEvent1;

        private SetStateActivity setStateActivity42;

        private SetStateActivity setStateActivity28;

        private HandleExternalEventActivity handleExternalEventActivity25;

        private TransactionScopeActivity transactionScopeActivity28;

        private HandleExternalEventActivity handleExternalEventActivity27;

        private EventDrivenActivity DenialEvent9;

        private EventDrivenActivity PaidEvent2;

        private CodeActivity InAccountingWithExportInitCode;

        private StateActivity InAccountingWithExport;

        private StateInitializationActivity InAccountingWithExportInit;

        private CodeActivity WriteCommentToHistoryCode;

        private SetStateActivity setStateActivity13;

        private IfElseBranchActivity ifElseBranchActivity1;

        private IfElseActivity ifElseActivity1;

        private TransactionScopeActivity transactionScopeActivity25;

        private SetStateActivity setStateActivity12;

        private TransactionScopeActivity transactionScopeActivity24;

        private HandleExternalEventActivity handleExternalEventActivity22;

        private EventDrivenActivity DenialEvent8;

        private StateActivity DraftForTechnicalDenial;

        private SetStateActivity setStateActivity41;

        private SetStateActivity setStateActivity40;

        private SetStateActivity setStateActivity39;

        private SetStateActivity setStateActivity38;

        private SetStateActivity setStateActivity37;

        private SetStateActivity setStateActivity36;

        private IfElseBranchActivity ifElseBranchActivity21;

        private IfElseBranchActivity ifElseBranchActivity20;

        private IfElseBranchActivity ifElseBranchActivity19;

        private IfElseBranchActivity ifElseBranchActivity18;

        private IfElseBranchActivity ifElseBranchActivity17;

        private IfElseBranchActivity ifElseBranchActivity16;

        private IfElseActivity ifElseActivity8;

        private CodeActivity PrepareForRoute1;

        private TransactionScopeActivity transactionScopeActivity23;

        private HandleExternalEventActivity handleExternalEventActivity21;

        private CodeActivity DraftForTechnicalDenialInitCode;

        private SetStateActivity setStateActivity1;

        private EventDrivenActivity StartProcessingEvent1;

        private StateInitializationActivity DraftForTechnicalDenialInit;

        private CodeActivity SetExternalParameters;

        private SetStateActivity setStateActivity33;

        private SetStateActivity setStateActivity35;

        private IfElseBranchActivity ifElseBranchActivity15;

        private FaultHandlerActivity faultHandlerActivity1;

        private FaultHandlersActivity faultHandlersActivity3;

        private CodeActivity UPKZHeadSightingInitCode;

        private StateInitializationActivity UPKZHeadSightingInit;

        private CodeActivity ResetLimitExecutorSightsCode1;

        private SetStateActivity setStateActivity21;

        private CodeActivity ResetLimitManagerSightsCode1;

        private IfElseBranchActivity ifElseBranchActivity6;

        private IfElseBranchActivity ifElseBranchActivity5;

        private SetStateActivity setStateActivity20;

        private SetStateActivity setStateActivity19;

        private SetStateActivity setStateActivity18;

        private CodeActivity ResetLimitExecutorSightsCode2;

        private IfElseActivity ifElseActivity4;

        private CodeActivity ResetLimitManagerSightsCode2;

        private TransactionScopeActivity transactionScopeActivity18;

        private HandleExternalEventActivity handleExternalEventActivity16;

        private TransactionScopeActivity transactionScopeActivity17;

        private HandleExternalEventActivity handleExternalEventActivity15;

        private TransactionScopeActivity transactionScopeActivity16;

        private HandleExternalEventActivity handleExternalEventActivity14;

        private EventDrivenActivity TechnicalDenialEvent6;

        private EventDrivenActivity DenialEvent6;

        private EventDrivenActivity SightingEvent6;

        private IfElseBranchActivity ifElseBranchActivity4;

        private IfElseActivity ifElseActivity3;

        private IfElseBranchActivity CheckLimitExecutorSight;

        private IfElseActivity ifElseActivity2;

        private SetStateActivity setStateActivity17;

        private SetStateActivity setStateActivity16;

        private SetStateActivity setStateActivity9;

        private CodeActivity LimitExecutorSightingInitCode;

        private TransactionScopeActivity transactionScopeActivity15;

        private HandleExternalEventActivity handleExternalEventActivity13;

        private TransactionScopeActivity transactionScopeActivity14;

        private HandleExternalEventActivity handleExternalEventActivity12;

        private TransactionScopeActivity transactionScopeActivity9;

        private HandleExternalEventActivity handleExternalEventActivity11;

        private StateInitializationActivity LimitExecutorSightingInit;

        private EventDrivenActivity TechnicalDenialEvent4;

        private EventDrivenActivity DenialEvent4;

        private EventDrivenActivity SightingEvent4;

        private StateActivity LimitExecutorSighting;

        private StateActivity UPKZHeadSighting;

        private SetStateActivity setStateActivity15;

        private TransactionScopeActivity transactionScopeActivity13;

        private HandleExternalEventActivity handleExternalEventActivity10;

        private EventDrivenActivity DenialEvent5;

        private CodeActivity OnPaymentInitCode;

        private StateInitializationActivity OnPaymentInit;

        private StateActivity OnPayment;

        private SetStateActivity setStateActivity11;

        private SetStateActivity setStateActivity10;

        private SetStateActivity setStateActivity6;

        private SetStateActivity setStateActivity4;

        private TransactionScopeActivity transactionScopeActivity11;

        private HandleExternalEventActivity handleExternalEventActivity9;

        private TransactionScopeActivity transactionScopeActivity10;

        private HandleExternalEventActivity handleExternalEventActivity8;

        private TransactionScopeActivity transactionScopeActivity6;

        private HandleExternalEventActivity handleExternalEventActivity6;

        private TransactionScopeActivity transactionScopeActivity4;

        private HandleExternalEventActivity handleExternalEventActivity4;

        private EventDrivenActivity TechnicalDenialEvent3;

        private EventDrivenActivity DenialEvent3;

        private EventDrivenActivity TechnicalDenialEvent2;

        private EventDrivenActivity DenialEvent2;

        private StateActivity PostingAccounting;

        private StateActivity UPKZCuratorSighting;

        private StateActivity LimitManagerSighting;

        private StateActivity UPKZCntrollerSighting;

        private StateActivity Archived;

        private HandleExternalEventActivity handleExternalEventActivity1;

        private EventDrivenActivity StartProcessingEvent;

        private SetStateActivity setStateActivity2;

        private TransactionScopeActivity transactionScopeActivity3;

        private HandleExternalEventActivity handleExternalEventActivity3;

        private TransactionScopeActivity transactionScopeActivity2;

        private HandleExternalEventActivity handleExternalEventActivity2;

        private TransactionScopeActivity transactionScopeActivity1;

        private EventDrivenActivity SightingEvent2;

        private EventDrivenActivity SightingEvent1;

        private SetStateActivity setStateActivity5;

        private TransactionScopeActivity transactionScopeActivity7;

        private HandleExternalEventActivity handleExternalEventActivity7;

        private TransactionScopeActivity transactionScopeActivity5;

        private HandleExternalEventActivity handleExternalEventActivity5;

        private EventDrivenActivity PostingAccountingEvent;

        private EventDrivenActivity SightingEvent3;

        private CodeActivity DraftInitCode;

        private StateInitializationActivity DraftInit;

        private CodeActivity UPKZCntrollerSightingInitCode;

        private StateInitializationActivity UPKZCntrollerSightingInit;

        private CodeActivity LimitManagerSightingInitCode;

        private StateInitializationActivity LimitManagerSightingInit;

        private CodeActivity UPKZCuratorSightingInitCode;

        private StateInitializationActivity UPKZCuratorSightingInit;

        private CodeActivity PostingAccountingInitCode;

        private StateInitializationActivity PostingAccountingInit;

        private HandleExternalEventActivity denialEventFired1;

        private EventDrivenActivity DenialEvent1;

        private SetStateActivity setStateActivity8;

        private TransactionScopeActivity transactionScopeActivity8;

        private SetStateActivity setStateActivity3;

        private StateActivity HeadInitiatorSighting;

        private SetStateActivity setStateActivity26;

        private SetStateActivity setStateActivity25;

        private SetStateActivity setStateActivity24;

        private TransactionScopeActivity transactionScopeActivity21;

        private HandleExternalEventActivity handleExternalEventActivity19;

        private TransactionScopeActivity transactionScopeActivity20;

        private HandleExternalEventActivity handleExternalEventActivity18;

        private TransactionScopeActivity transactionScopeActivity19;

        private HandleExternalEventActivity handleExternalEventActivity17;

        private CodeActivity HeadInitiatorSightingInitCode;

        private EventDrivenActivity SightingEvent7;

        private EventDrivenActivity DenialEvent7;

        private EventDrivenActivity TechnicalDenialEvent7;

        private StateInitializationActivity HeadInitiatorSightingInit;

        private SetStateActivity setStateActivity27;

        private IfElseBranchActivity ifElseBranchActivity10;

        private IfElseBranchActivity CheckInitiatorHeadMustSign;

        private IfElseActivity ifElseActivity5;

        private StateActivity Paid;

        private StateInitializationActivity PaidInit;

        private CodeActivity PaidInitCode;

        private SetStateActivity setStateActivity29;

        private TransactionScopeActivity transactionScopeActivity22;

        private SetStateActivity setStateActivity30;

        private HandleExternalEventActivity handleExternalEventActivity20;

        private SetStateActivity setStateActivity14;

        private DelayActivity delayActivity1;

        private StateInitializationActivity OnPaymentCheckStatusInit;

        private EventDrivenActivity CheckStatusManually;

        private EventDrivenActivity CheckStatusOnTimer;

        private StateActivity OnPaymentCheckStatus;

        private SequenceActivity sequenceActivity1;

        private FaultHandlersActivity faultHandlersActivity1;

        private IfElseBranchActivity ifElseBranchActivity12;

        private IfElseBranchActivity ifElseBranchActivity11;

        private IfElseActivity ifElseActivity6;

        private SetStateActivity setStateActivity32;

        private SetStateActivity setStateActivity31;

        private FaultHandlersActivity faultHandlersActivity2;

        private TransactionScopeActivity transactionScopeActivity12;

        private CodeActivity PrepareForRouteCode;

        private CodeActivity CheckExternalStatus;

        private StateActivity Draft;



















































































































































































































































































































































    }
}
