db.WorkflowInbox.createIndex({IdentityId: 1});
db.WorkflowInbox.createIndex({ProcessId: 1});

db.WorkflowProcessInstance.createIndex({RootProcessId: 1});

db.WorkflowProcessInstancePersistence.createIndex({ProcessId: 1});

db.WorkflowProcessInstanceStatus.createIndex({Status: 1});
db.WorkflowProcessInstanceStatus.createIndex({Status: 1, RuntimeId: 1});

db.WorkflowProcessScheme.createIndex({DefiningParametersHash: 1});
db.WorkflowProcessScheme.createIndex({SchemeCode: 1});
db.WorkflowProcessScheme.createIndex({IsObsolete: 1});

db.WorkflowProcessTimer.createIndex({ProcessId: 1});
db.WorkflowProcessTimer.createIndex({Name: 1});
db.WorkflowProcessTimer.createIndex({NextExecutionDateTime: 1});
db.WorkflowProcessTimer.createIndex({Ignore: 1});

db.WorkflowProcessTransitionHistory.createIndex({ProcessId: 1});
db.WorkflowProcessTransitionHistory.createIndex({ExecutorIdentityId: 1});
db.WorkflowProcessTransitionHistory.createIndex({ActorIdentityId: 1});

db.WorkflowScheme.createIndex({Code: 1});

db.WorkflowGlobalParameter.createIndex({Type: 1});
db.WorkflowGlobalParameter.createIndex({Name: 1});

db.WorkflowRuntime.createIndex({RuntimeId: 1});

db.WorkflowSync.createIndex({Name: 1});

db.WorkflowApprovalHistory.createIndex({ProcessId: 1});
db.WorkflowApprovalHistory.createIndex({IdentityId: 1});