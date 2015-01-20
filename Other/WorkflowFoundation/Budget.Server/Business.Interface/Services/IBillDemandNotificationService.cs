using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IBillDemandNotificationService
    {
        void CheckAndSendMailForState(Guid billDemandUid, WorkflowState state);
        void CheckAndSendMail(Guid billDemandUid);
        void SendNotificationsForState(Guid billDemandUid, WorkflowState state);
    }
}
