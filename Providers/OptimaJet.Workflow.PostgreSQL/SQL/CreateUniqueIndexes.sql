CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowInbox_ProcessId_IdentityId_idx" ON "WorkflowInbox" USING btree ("ProcessId", "IdentityId");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessInstancePersistence_ProcessId_ParameterName_idx" ON "WorkflowProcessInstancePersistence" USING btree ("ProcessId", "ParameterName");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowProcessTimer_ProcessId_Name_idx" ON "WorkflowProcessTimer" USING btree ("ProcessId", "Name");
CREATE UNIQUE INDEX IF NOT EXISTS "WorkflowGlobalParameter_Type_Name_idx" ON "WorkflowGlobalParameter" USING btree ("Type", "Name");
