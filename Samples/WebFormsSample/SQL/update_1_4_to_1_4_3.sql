ALTER TABLE WorkflowProcessInstance ALTER COLUMN StateName nvarchar(max) NULL
ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ExecutorIdentityId nvarchar(max) NULL
ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN ActorIdentityId nvarchar(max) NULL
ALTER TABLE WorkflowProcessTransitionHistory ALTER COLUMN FromStateName nvarchar(max) NULL