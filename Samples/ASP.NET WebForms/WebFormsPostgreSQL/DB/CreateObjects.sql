/*
Company: OptimaJet
Project: WF.Sample WorkflowEngine.NET
File: CreateObjects.sql
*/


CREATE TABLE IF NOT EXISTS "StructDivision"
(
	  "Id" uuid NOT NULL,
	  "Name" character varying(256) NOT NULL,
	  "ParentId" uuid NULL REFERENCES "StructDivision" ON DELETE CASCADE,
	  CONSTRAINT "PK_StructDivision" PRIMARY KEY ("Id")
);


CREATE OR REPLACE VIEW "vStructDivisionParents"
	AS
	WITH RECURSIVE "cteRecursive" as (
	 select sd."Id" AS "FirstId", sd."ParentId" as "ParentId", sd."Id" AS "Id"
	  from  "StructDivision" AS sd WHERE sd."ParentId" IS NOT NULL
	 union all 
	 select r."FirstId" AS "FirstId", sdr."ParentId" AS "ParentId", sdr."Id" AS "Id"
	 from "StructDivision" AS sdr
	 inner join "cteRecursive" AS r ON r."ParentId" = sdr."Id")
	select DISTINCT "FirstId" AS "Id", "ParentId" AS "ParentId" FROM "cteRecursive"; 

CREATE OR REPLACE VIEW "vStructDivisionParentsAndThis"
	AS
	select  "Id" AS "Id", "Id" AS "ParentId" FROM "StructDivision"
	UNION 
	select  "Id" AS "Id", "ParentId" AS "ParentId" FROM "vStructDivisionParents";

CREATE TABLE IF NOT EXISTS "Employee" (
	  "Id" uuid NOT NULL,
	  "Name" character varying(256) NOT NULL,
	  "StructDivisionId" uuid NOT NULL REFERENCES "StructDivision" ON DELETE CASCADE,
	  "IsHead" boolean NOT NULL DEFAULT 0::boolean,
	  CONSTRAINT "PK_Employee" PRIMARY KEY ("Id")
	);

CREATE OR REPLACE VIEW "vHeads"
	AS
	select  e."Id" AS "Id", e."Name" AS "Name", eh."Id" AS "HeadId", eh."Name" AS "HeadName" FROM "Employee" AS e 
		INNER JOIN "vStructDivisionParentsAndThis" AS vsp ON e."StructDivisionId" = vsp."Id"
		INNER JOIN "Employee" AS eh ON eh."StructDivisionId" = vsp."ParentId" AND eh."IsHead" = true;


CREATE TABLE IF NOT EXISTS "Document" (
	  "Id" uuid NOT NULL,
	  "Number" SERIAL,
	  "Name" character varying(256) NOT NULL,
	  "Comment" text NULL,
	  "AuthorId" uuid NOT NULL REFERENCES "Employee",
	  "ManagerId" uuid NULL REFERENCES "Employee",
	  "Sum" money NOT NULL DEFAULT 0,
	  "State" character varying(1024) NOT NULL DEFAULT 'VacationRequestCreated',
	  "StateName" character varying(1024),
	  CONSTRAINT "PK_Document" PRIMARY KEY ("Id")
	);

CREATE TABLE IF NOT EXISTS "DocumentTransitionHistory" (
	  "Id" uuid NOT NULL,
	  "DocumentId" uuid NOT NULL REFERENCES "Document" ON DELETE CASCADE,
	  "EmployeeId" uuid NULL REFERENCES "Employee",
	  "AllowedToEmployeeNames" text NOT NULL,
	  "TransitionTime" timestamp NULL,
	  "Order" SERIAL,
	  "InitialState" character varying(1024) NOT NULL,
	  "DestinationState" character varying(1024) NOT NULL,
	  "Command" character varying(1024) NOT NULL,
	  CONSTRAINT "PK_DocumentTransitionHistory" PRIMARY KEY ("Id")
	);

CREATE TABLE IF NOT EXISTS "Roles" (
	  "Id" uuid NOT NULL,
	  "Name" character varying(256) NOT NULL,
	  CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
	);

CREATE TABLE IF NOT EXISTS "EmployeeRole" (
	  "EmployeeId" uuid NOT NULL REFERENCES "Employee" ON DELETE CASCADE,
	  "RoleId" uuid NOT NULL REFERENCES "Roles" ON DELETE CASCADE,
	  CONSTRAINT "PK_EmployeeRoles" PRIMARY KEY ("EmployeeId", "RoleId")
);