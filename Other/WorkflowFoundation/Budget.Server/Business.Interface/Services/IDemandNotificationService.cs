using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.DAL.DataContracts;

namespace Budget2.Server.Business.Interface.Services
{
    public interface IDemandNotificationService
    {
        void SendNotificationsForState(Guid demandUid, WorkflowState state);
    }
}
