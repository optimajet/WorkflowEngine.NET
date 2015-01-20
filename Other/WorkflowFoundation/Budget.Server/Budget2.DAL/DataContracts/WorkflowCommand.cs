using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    [Serializable]
    public class WorkflowCommand
    {
        public Guid Id { get; private set; }
        public WorkflowCommand (Guid id)
        {
            Id = id;
        }

        public bool SkipCheckCommandId { get; set; }

        public static readonly WorkflowCommand Sighting = new WorkflowCommand(new Guid("C2855FA3-B289-472D-BA6B-1CBC6F8A21FD"));
        public static readonly WorkflowCommand StartProcessing = new WorkflowCommand(new Guid("EF6792A3-1097-4A38-BA1A-A0DF770065BA"));
        public static readonly WorkflowCommand Denial = new WorkflowCommand(new Guid("BF7DA0A3-BDBF-4835-A35B-F30D87E9B3CE"));
        public static readonly WorkflowCommand DenialByTechnicalCauses = new WorkflowCommand(new Guid("C99210CA-C622-40D7-A7D3-DDA4935C6F42"));
        public static readonly WorkflowCommand Rollback = new WorkflowCommand(new Guid("0B7D5B57-B587-470E-A5A4-E59D6FED59E2"));
        public static readonly WorkflowCommand SetPaid = new WorkflowCommand(new Guid("4F663178-953F-4232-843E-96E231382DB3")){SkipCheckCommandId = true};
        public static readonly WorkflowCommand Export = new WorkflowCommand(new Guid("0E0051B7-3027-49E1-BF57-096821C36584")) { SkipCheckCommandId = true };
        public static readonly WorkflowCommand Unknown = new WorkflowCommand(Guid.Empty) { SkipCheckCommandId = true };

        private static readonly Dictionary<Guid, string> _commandNames = new Dictionary<Guid, string>()
                                                                             {
                                                                                 {Sighting.Id, "Утвердить"},
                                                                                 {Denial.Id, "Отказать"},
                                                                                 {DenialByTechnicalCauses.Id,"Отказать по ТП"},
                                                                                 {SetPaid.Id,"Подтвердить оплату"},
                                                                                 {StartProcessing.Id,"Отправить на маршрут"},
                                                                                 {Export.Id,"Отправить на оплату"},
                                                                                 {Rollback.Id,"Отозвать"}
                                                                             };

        public static string GetCommandDescription (WorkflowCommand command, string nextStateName )
        {
            string description;
            if (_commandNames.TryGetValue(command.Id,out description))
                return description;

            return string.Format("Изменить на {0}", nextStateName);
        }

       }
}
