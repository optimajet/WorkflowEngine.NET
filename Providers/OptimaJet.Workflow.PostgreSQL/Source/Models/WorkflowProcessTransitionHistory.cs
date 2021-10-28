﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using OptimaJet.Workflow.Core.Persistence;

// ReSharper disable once CheckNamespace
namespace OptimaJet.Workflow.PostgreSQL
{
    public class WorkflowProcessTransitionHistory : DbObject<WorkflowProcessTransitionHistory>
    {
        public string ActorIdentityId { get; set; }
        public string ExecutorIdentityId { get; set; }
        public string ActorName { get; set; }
        public string ExecutorName { get; set; }
        public string FromActivityName { get; set; }
        public string FromStateName { get; set; }
        public Guid Id { get; set; }
        public bool IsFinalised { get; set; }
        public Guid ProcessId { get; set; }
        public string ToActivityName { get; set; }
        public string ToStateName { get; set; }
        public string TransitionClassifier { get; set; }
        public DateTime TransitionTime { get; set; }
        public string TriggerName { get; set; }
        public DateTime? StartTransitionTime { get; set; }
        public long? TransitionDuration { get; set; }

        static WorkflowProcessTransitionHistory()
        {
            DbTableName = "WorkflowProcessTransitionHistory";
        }

        public WorkflowProcessTransitionHistory()
        {
            DBColumns.AddRange(new[]
            {
                new ColumnInfo {Name = "Id", IsKey = true, Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "ActorIdentityId"},
                new ColumnInfo {Name = "ExecutorIdentityId"},
                new ColumnInfo {Name = "ActorName"},
                new ColumnInfo {Name = "ExecutorName"},
                new ColumnInfo {Name = "FromActivityName"},
                new ColumnInfo {Name = "FromStateName"},
                new ColumnInfo {Name = "IsFinalised", Type = NpgsqlDbType.Boolean},
                new ColumnInfo {Name = "ProcessId", Type = NpgsqlDbType.Uuid},
                new ColumnInfo {Name = "ToActivityName"},
                new ColumnInfo {Name = "ToStateName"},
                new ColumnInfo {Name = "TransitionClassifier"},
                new ColumnInfo {Name = "TransitionTime", Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = "TriggerName"},
                new ColumnInfo {Name = "StartTransitionTime", Type = NpgsqlDbType.Timestamp},
                new ColumnInfo {Name = "TransitionDuration", Type = NpgsqlDbType.Bigint},
            });
        }

        public override object GetValue(string key)
        {
            return key switch
            {
                "Id" => Id,
                "ActorIdentityId" => ActorIdentityId,
                "ExecutorIdentityId" => ExecutorIdentityId,
                "ActorName" => ActorName,
                "ExecutorName" => ExecutorName,
                "FromActivityName" => FromActivityName,
                "FromStateName" => FromStateName,
                "IsFinalised" => IsFinalised,
                "ProcessId" => ProcessId,
                "ToActivityName" => ToActivityName,
                "ToStateName" => ToStateName,
                "TransitionClassifier" => TransitionClassifier,
                "TransitionTime" => TransitionTime,
                "TriggerName" => TriggerName,
                "StartTransitionTime" => StartTransitionTime,
                "TransitionDuration" => TransitionDuration,
                _ => throw new Exception($"Column {key} is not exists")
            };
        }

        public override void SetValue(string key, object value)
        {
            switch (key)
            {
                case "Id":
                    Id = (Guid)value;
                    break;
                case "ActorIdentityId":
                    ActorIdentityId = value as string;
                    break;
                case "ExecutorIdentityId":
                    ExecutorIdentityId = value as string;
                    break;
                case "ActorName":
                    ActorName = value as string;
                    break;
                case "ExecutorName":
                    ExecutorName = value as string;
                    break;
                case "FromActivityName":
                    FromActivityName = value as string;
                    break;
                case "FromStateName":
                    FromStateName = value as string;
                    break;
                case "IsFinalised":
                    IsFinalised = (bool)value;
                    break;
                case "ProcessId":
                    ProcessId = (Guid)value;
                    break;
                case "ToActivityName":
                    ToActivityName = value as string;
                    break;
                case "ToStateName":
                    ToStateName = value as string;
                    break;
                case "TransitionClassifier":
                    TransitionClassifier = value as string;
                    break;
                case "TransitionTime":
                    TransitionTime = (DateTime)value;
                    break;
                case "TriggerName":
                    TriggerName = value as string;
                    break;
                case "StartTransitionTime":
                    StartTransitionTime = value as DateTime?;
                    break;
                case "TransitionDuration":
                    TransitionDuration = value as long?;
                    break;
                default:
                    throw new Exception($"Column {key} is not exists");
            }
        }

        public static async Task<int> DeleteByProcessIdAsync(NpgsqlConnection connection, Guid processId, NpgsqlTransaction transaction = null)
        {
            return await DeleteByAsync(connection, x => x.ProcessId, processId, transaction)
                .ConfigureAwait(false);
        }

        public static async Task<WorkflowProcessTransitionHistory []> SelectByProcessIdAsync(NpgsqlConnection connection, Guid processId, Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ProcessId, processId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
        
        public static async Task<WorkflowProcessTransitionHistory []>SelectByIdentityIdAsync(NpgsqlConnection connection, string identityId, Paging paging = null)
        {
            return await SelectByWithPagingAsync(connection, x => x.ExecutorIdentityId, identityId, x => x.TransitionTime,
                SortDirection.Desc, paging).ConfigureAwait(false);
        }
        
    }
}
