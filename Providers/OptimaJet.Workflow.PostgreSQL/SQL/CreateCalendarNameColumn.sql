ALTER TABLE "WorkflowProcessInstance"
    ADD COLUMN IF NOT EXISTS "CalendarName" character varying(256) null;

CREATE INDEX IF NOT EXISTS "IX_CalendarName" ON "WorkflowProcessInstance" USING btree ("CalendarName");
