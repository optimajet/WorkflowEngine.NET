using System.Runtime.Serialization;

namespace Budget2.Server.Security.Interface.DataContracts
{
    [DataContract]
    public enum WorkflowCommandType
    {
        [EnumMember]
        StartProcessing,
        [EnumMember]
        Sighting,
        [EnumMember]
        PostingAccounting,
        [EnumMember]
        Denial,
        [EnumMember]
        DenialByTechnicalCauses,
        [EnumMember]
        SetPaidStatus,
        [EnumMember]
        SetDenialStatus,
        [EnumMember]
        Export,
        [EnumMember]
        CheckStatus,
        [EnumMember]
        SetWorkflowState
    }
}
