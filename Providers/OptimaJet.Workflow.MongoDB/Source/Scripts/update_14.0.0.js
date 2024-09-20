db.WorkflowInbox.createIndex({ProcessId: 1, IdentityId: 1}, {unique: true});
db.WorkflowProcessInstancePersistence.createIndex({ProcessId: 1, ParameterName: 1}, {unique: true});
db.WorkflowProcessTimer.createIndex({ProcessId: 1, Name: 1}, {unique: true});
db.WorkflowGlobalParameter.createIndex({Type: 1, Name: 1}, {unique: true});
db.WorkflowScheme.dropIndex({Code: 1});
db.WorkflowScheme.createIndex({Code: 1}, {unique: true});