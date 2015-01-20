using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.Workflow.Interface.DataContracts;
using Budget2.Server.Workflow.Interface.Services;
using Common.Utils;

namespace Budget2.Server.Workflow.Services
{
    public class WorkflowParcelService : IWorkflowParcelService
    {
        private DictionaryCache<Guid, WorkflowSetStateParcel> _workflowSetStateParcelCache;
        private DictionaryCache<Guid, string> _workflowErrorMessageCache;

        public WorkflowParcelService()
        {
            _workflowSetStateParcelCache = new DictionaryCache<Guid, WorkflowSetStateParcel>(new TimeSpan(0, 5, 0),
                                                                                     FillParcelCache);
            _workflowErrorMessageCache = new DictionaryCache<Guid, string>(new TimeSpan(0, 5, 0), FillErrorMessageCache);

        }

        private Dictionary<Guid, string> FillErrorMessageCache()
        {
            return new Dictionary<Guid, string>();
        }

        private Dictionary<Guid, WorkflowSetStateParcel> FillParcelCache()
        {
            return new Dictionary<Guid, WorkflowSetStateParcel>();
        }

        public void AddErrorMessageParcel (Guid id, string message)
        {
            _workflowErrorMessageCache.AddValue(id,message);
        }

        public string GetAndRemoveMessage (Guid id)
        {
            var value = _workflowErrorMessageCache.GetValue(id);
            _workflowErrorMessageCache.RemoveValue(id);
            return value;
        }

        public void AddParcel(Guid id, WorkflowSetStateParcel value)
        {
            _workflowSetStateParcelCache.AddValue(id,value);
        }

        public WorkflowSetStateParcel GetAndRemoveParcel(Guid id)
        {
            var value = _workflowSetStateParcelCache.GetValue(id);
            _workflowSetStateParcelCache.RemoveValue(id);
            return value;
        }
    }
}
