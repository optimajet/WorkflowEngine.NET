using System.Collections.Generic;

namespace OptimaJet.Workflow.Core.Persistence
{
    /// <summary>
    /// Represent a stage of a life cycle of a process
    /// </summary>
    public sealed class ProcessStatus
    {
        public byte Id { get; private set; }
        public bool IsAllowedToChangeStatus { get; set; }
        public bool IsAllowedToExecuteCommand { get; set; }

        /// <summary>
        /// Status of a processes which are not exists in persistence store
        /// </summary>
        public static readonly ProcessStatus NotFound = new ProcessStatus
        {
            Id = 255,
            IsAllowedToChangeStatus = false,
            IsAllowedToExecuteCommand = false
        };

        /// <summary>
        /// Status of a processes which are exists in persistence store but theirs status is not defined
        /// </summary>
        public static readonly ProcessStatus Unknown = new ProcessStatus
        {
            Id = 254,
            IsAllowedToChangeStatus = false,
            IsAllowedToExecuteCommand = false
        };

        /// <summary>
        /// Status of a processes which was created just now 
        /// </summary>
        public static readonly ProcessStatus Initialized = new ProcessStatus
                                                                {
                                                                    Id = 0,
                                                                    IsAllowedToChangeStatus = false,
                                                                    IsAllowedToExecuteCommand = false
                                                                };
        /// <summary>
        /// Status of a processes which are executing at current moment
        /// </summary>
        public static readonly ProcessStatus Running = new ProcessStatus
                                                            {
                                                                Id = 1,
                                                                IsAllowedToChangeStatus = false,
                                                                IsAllowedToExecuteCommand = false
                                                            };
        /// <summary>
        /// Status of a processes which are not executing at current moment and awaiting an external interaction
        /// </summary>
        public static readonly ProcessStatus Idled = new ProcessStatus
                                                          {
                                                              Id = 2,
                                                              IsAllowedToChangeStatus = true,
                                                              IsAllowedToExecuteCommand = true
                                                          };
        /// <summary>
        /// Status of a processes which was finalized
        /// </summary>
        public static readonly ProcessStatus Finalized = new ProcessStatus
        {
            Id = 3,
            IsAllowedToChangeStatus = true,
            IsAllowedToExecuteCommand = false
        };

        /// <summary>
        /// Status of a processes which was terminated with an error
        /// </summary>
        public static readonly ProcessStatus Terminated = new ProcessStatus
        {
            Id = 4,
            IsAllowedToChangeStatus = true,
            IsAllowedToExecuteCommand = false
        };

        /// <summary>
        /// Status of a processes which had an error but not terminated
        /// </summary>
        public static readonly ProcessStatus Error = new ProcessStatus
        {
            Id = 5,
            IsAllowedToChangeStatus = true,
            IsAllowedToExecuteCommand = true
        };

        public static readonly IEnumerable<ProcessStatus> All = new List<ProcessStatus>
                                                                     {Initialized, Running, Idled, Finalized, Terminated, Error};
    }
}
