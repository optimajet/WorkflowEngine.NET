using System;

namespace Budget2.DAL.DataContracts
{
    [Serializable]
    public class WorkflowState
    {
        public string WorkflowStateName { get; set; }

        public WorkflowType Type { get; set; }

        public byte? DbStateId { get; set; }

        public string StateNamePostfix { get; set; }

        public bool IsInitial { get; set; }

        public bool IsFinal { get; set; }

        public bool ExcludeFromSetStateCommand { get; set; }

        public byte? Order { get; set; }

        public static readonly WorkflowState BillDemandDraft = new WorkflowState
                                                                   {
                                                                       Type = WorkflowType.BillDemandWorkfow,
                                                                       WorkflowStateName = "Draft",
                                                                       DbStateId = 1, 
                                                                       IsInitial = true
                                                                       , Order = 1
           
                                                                   };

        public static readonly WorkflowState BillDemandDraftForTechnicalDenial = new WorkflowState
        {
            Type = WorkflowType.BillDemandWorkfow,
            WorkflowStateName = "DraftForTechnicalDenial",
            DbStateId = 1,
            StateNamePostfix = "по ТП",
            Order = 1
        };

        //public static readonly WorkflowState BillDemandInAccounting = new WorkflowState
        //{
        //    Type =
        //        WorkflowType.
        //        BillDemandWorkfow,
        //    WorkflowStateName =
        //        "InAccounting",
        //    DbStateId = 11
        //};


        public static readonly WorkflowState BillDemandInAccountingWithExport = new WorkflowState
        {
            Type =
                WorkflowType.
                BillDemandWorkfow,
            WorkflowStateName =
                "InAccountingWithExport",
            DbStateId = 11,
            Order = 9
        };

        public static readonly WorkflowState BillDemandUPKZCntrollerSighting = new WorkflowState
                                                                                   {
                                                                                       Type =
                                                                                           WorkflowType.
                                                                                           BillDemandWorkfow,
                                                                                       WorkflowStateName =
                                                                                           "UPKZCntrollerSighting",
                                                                                       DbStateId = 2
                                                                                       ,
                                                                                       Order = 2
                                                                                   };

        public static readonly WorkflowState BillDemandHeadInitiatorSighting = new WorkflowState
                                                                                   {
                                                                                       Type =
                                                                                           WorkflowType.
                                                                                           BillDemandWorkfow,
                                                                                       WorkflowStateName =
                                                                                           "HeadInitiatorSighting",
                                                                                       DbStateId = 3
                                                                                       ,
                                                                                       Order = 3
                                                                                   };

        public static readonly WorkflowState BillDemandLimitExecutorSighting = new WorkflowState
                                                                                   {
                                                                                       Type =
                                                                                           WorkflowType.
                                                                                           BillDemandWorkfow,
                                                                                       WorkflowStateName =
                                                                                           "LimitExecutorSighting",
                                                                                       DbStateId = 4
                                                                                       ,
                                                                                       Order = 4
                                                                                   };

        public static readonly WorkflowState BillLimitManagerSighting = new WorkflowState
                                                                            {
                                                                                Type = WorkflowType.BillDemandWorkfow,
                                                                                WorkflowStateName =
                                                                                    "LimitManagerSighting",
                                                                                DbStateId = 5,
                                                                                Order = 5
                                                                            };

        public static readonly WorkflowState BillDemandUPKZCuratorSighting = new WorkflowState
                                                                                 {
                                                                                     Type =
                                                                                         WorkflowType.BillDemandWorkfow,
                                                                                     WorkflowStateName =
                                                                                         "UPKZCuratorSighting",
                                                                                     DbStateId = 6 , Order = 6
                                                                                 };

        public static readonly WorkflowState BillDemandUPKZHeadSighting = new WorkflowState
                                                                              {
                                                                                  Type = WorkflowType.BillDemandWorkfow,
                                                                                  WorkflowStateName = "UPKZHeadSighting",
                                                                                  DbStateId = 7,Order = 7
                                                                              };

        public static readonly WorkflowState BillDemandPostingAccounting = new WorkflowState
                                                                               {
                                                                                   Type = WorkflowType.BillDemandWorkfow,
                                                                                   WorkflowStateName =
                                                                                       "PostingAccounting",
                                                                                   DbStateId = 8, Order = 8
                                                                               };

        public static readonly WorkflowState BillDemandInitiatorConfirmation = new WorkflowState
        {
            Type = WorkflowType.BillDemandWorkfow,
            ExcludeFromSetStateCommand = true,
            WorkflowStateName =
                "InitiatorConfirmation",
            DbStateId = 8,
            StateNamePostfix = "- Филиалы", Order = 8
        };

        public static readonly WorkflowState BillDemandOnPayment = new WorkflowState
                                                                       {
                                                                           Type = WorkflowType.BillDemandWorkfow,
                                                                           WorkflowStateName = "OnPayment",
                                                                           DbStateId = 9, Order = 10
                                                                       };

        public static readonly WorkflowState BillDemandPaid = new WorkflowState
                                                                  {
                                                                      Type = WorkflowType.BillDemandWorkfow,
                                                                      WorkflowStateName = "Paid",
                                                                      DbStateId = 10, Order = 11, IsFinal = true
                                                                  };

        public static readonly WorkflowState BillDemandArchived = new WorkflowState
                                                                      {
                                                                          Type = WorkflowType.BillDemandWorkfow,
                                                                          WorkflowStateName = "Archived"
                                                                      };

        public static readonly WorkflowState DemandAdjustmentDraft = new WorkflowState
                                                                         {
                                                                             Type =
                                                                                 WorkflowType.DemandAdjustmentWorkflow,
                                                                             WorkflowStateName = "Draft",
                                                                             DbStateId = 1,
                                                                             IsInitial = true,
                                                                             Order = 1
                                                                         };

        public static readonly WorkflowState DemandAdjustmentSourceDemandLimitExecutorSighting = new WorkflowState
                                                                                                     {
                                                                                                         Type =
                                                                                                             WorkflowType
                                                                                                             .
                                                                                                             DemandAdjustmentWorkflow,
                                                                                                         WorkflowStateName
                                                                                                             =
                                                                                                             "SourceDemandLimitExecutorSighting",
                                                                                                         DbStateId = 2,  Order = 3
                                                                                                     };

        //public static readonly WorkflowState DemandAdjustmentDemandLimitManagerSighting = new WorkflowState
        //                                                                                      {
        //                                                                                          Type =
        //                                                                                              WorkflowType.
        //                                                                                              DemandAdjustmentWorkflow,
        //                                                                                          WorkflowStateName =
        //                                                                                              "DemandLimitManagerSighting",
        //                                                                                          DbStateId = 3
        //                                                                                      };

        public static readonly WorkflowState DemandAdjustmentUPKZCuratorSighting = new WorkflowState
                                                                                       {
                                                                                           Type =
                                                                                               WorkflowType.
                                                                                               DemandAdjustmentWorkflow,
                                                                                           WorkflowStateName =
                                                                                               "UPKZCuratorSighting",
                                                                                           DbStateId = 4,  Order = 7
                                                                                       };

        public static readonly WorkflowState DemandAdjustmentSourceDemandLimitManagerSighting = new WorkflowState
                                                                                                    {
                                                                                                        Type =
                                                                                                            WorkflowType
                                                                                                            .
                                                                                                            DemandAdjustmentWorkflow,
                                                                                                        WorkflowStateName
                                                                                                            =
                                                                                                            "SourceDemandLimitManagerSighting",
                                                                                                        DbStateId = 5,  Order = 4
                                                                                                    };

        public static readonly WorkflowState DemandAdjustmentTargetDemandLimitExecutorSighting = new WorkflowState
                                                                                                     {
                                                                                                         Type =
                                                                                                             WorkflowType
                                                                                                             .
                                                                                                             DemandAdjustmentWorkflow,
                                                                                                         WorkflowStateName
                                                                                                             =
                                                                                                             "TargetDemandLimitExecutorSighting",
                                                                                                         DbStateId = 6, Order = 5
                                                                                                     };

        public static readonly WorkflowState DemandAdjustmentTargetDemandLimitManagerSighting = new WorkflowState
                                                                                                    {
                                                                                                        Type =
                                                                                                            WorkflowType
                                                                                                            .
                                                                                                            DemandAdjustmentWorkflow,
                                                                                                        WorkflowStateName
                                                                                                            =
                                                                                                            "TargetDemandLimitManagerSighting",
                                                                                                        DbStateId = 7, Order =6
                                                                                                    };

        public static readonly WorkflowState DemandAdjustmentUPKZHeadSighting = new WorkflowState
                                                                                    {
                                                                                        Type =
                                                                                            WorkflowType.
                                                                                            DemandAdjustmentWorkflow,
                                                                                        WorkflowStateName =
                                                                                            "UPKZHeadSighting",
                                                                                        DbStateId = 8, Order = 2
                                                                                    };


        public static readonly WorkflowState DemandAdjustmentAgreed = new WorkflowState
                                                                          {
                                                                              Type =
                                                                                  WorkflowType.DemandAdjustmentWorkflow,
                                                                              WorkflowStateName = "Agreed",
                                                                              DbStateId = 9,
                                                                              IsFinal = true, Order = 8
                                                                          };

        public static readonly WorkflowState DemandAdjustmentArchived = new WorkflowState
                                                                            {
                                                                                Type =
                                                                                    WorkflowType.
                                                                                    DemandAdjustmentWorkflow,
                                                                                WorkflowStateName = "Archived"
                                                                            };

        public static readonly WorkflowState DemandDraft = new WorkflowState
                                                               {
                                                                   Type =
                                                                       WorkflowType.DemandWorkflow,
                                                                   WorkflowStateName = "Draft",
                                                                   DbStateId = 1,
                                                                   IsInitial = true, Order = 1
                                                               };

        public static readonly WorkflowState DemandOPExpertSighting = new WorkflowState
                                                                          {
                                                                              Type =
                                                                                  WorkflowType.DemandWorkflow,
                                                                              WorkflowStateName = "OPExpertSighting",
                                                                              DbStateId = 2, Order = 2 
                                                                          };

        public static readonly WorkflowState DemandOPHeadSighting = new WorkflowState
                                                                        {
                                                                            Type =
                                                                                WorkflowType.DemandWorkflow,
                                                                            WorkflowStateName = "OPHeadSighting",
                                                                            DbStateId = 3, Order = 4
                                                                        };

        public static readonly WorkflowState DemandInitiatorHeadSighting = new WorkflowState
                                                                               {
                                                                                   Type =
                                                                                       WorkflowType.DemandWorkflow,
                                                                                   WorkflowStateName =
                                                                                       "InitiatorHeadSighting",
                                                                                   DbStateId = 4, Order = 3
                                                                               };

        public static readonly WorkflowState DemandUPKZCuratorSighting = new WorkflowState
                                                                             {
                                                                                 Type =
                                                                                     WorkflowType.DemandWorkflow,
                                                                                 WorkflowStateName =
                                                                                     "UPKZCuratorSighting",
                                                                                 DbStateId = 5, Order = 5
                                                                             };

        public static readonly WorkflowState DemandUPKZHeadSighting = new WorkflowState
                                                                          {
                                                                              Type =
                                                                                  WorkflowType.DemandWorkflow,
                                                                              WorkflowStateName = "UPKZHeadSighting",
                                                                              DbStateId = 6, Order = 6
                                                                          };

        public static readonly WorkflowState DemandAgreed = new WorkflowState
                                                                {
                                                                    Type =
                                                                        WorkflowType.DemandWorkflow,
                                                                    WorkflowStateName = "Agreed",
                                                                    DbStateId = 7,
                                                                    IsFinal = true, Order = 7
                                                                };

        public static readonly WorkflowState DemandArchived = new WorkflowState
                                                                  {
                                                                      Type =
                                                                          WorkflowType.DemandWorkflow,
                                                                      WorkflowStateName = "Archived"
                                                                  };


        public static readonly WorkflowState[] AllStates = new[]
                                                               {
                                                                   BillDemandDraft,
                                                                   BillDemandPaid,
                                                                   BillDemandDraftForTechnicalDenial,
                                                                   BillDemandUPKZCntrollerSighting,
                                                                   BillDemandHeadInitiatorSighting,
                                                                   BillDemandLimitExecutorSighting,
                                                                   BillLimitManagerSighting,
                                                                   BillDemandUPKZCuratorSighting,
                                                                   BillDemandUPKZHeadSighting,
                                                                   BillDemandPostingAccounting,
                                                                   BillDemandInitiatorConfirmation,
                                                                   BillDemandArchived,
                                                                   BillDemandOnPayment,
                                                                   BillDemandInAccountingWithExport,
                                                                   DemandAdjustmentDraft,
                                                                   DemandAdjustmentSourceDemandLimitExecutorSighting,
                                                                   DemandAdjustmentSourceDemandLimitManagerSighting,
                                                                   DemandAdjustmentUPKZCuratorSighting,
                                                                   DemandAdjustmentTargetDemandLimitExecutorSighting,
                                                                   DemandAdjustmentTargetDemandLimitManagerSighting,
                                                                   DemandAdjustmentUPKZHeadSighting,
                                                                   DemandAdjustmentAgreed,
                                                                   DemandAdjustmentArchived,
                                                                   DemandDraft,
                                                                   DemandOPExpertSighting,
                                                                   DemandOPHeadSighting,
                                                                   DemandInitiatorHeadSighting,
                                                                   DemandUPKZCuratorSighting,
                                                                   DemandUPKZHeadSighting,
                                                                   DemandAgreed,
                                                                   DemandArchived
                                                               };
    }
}