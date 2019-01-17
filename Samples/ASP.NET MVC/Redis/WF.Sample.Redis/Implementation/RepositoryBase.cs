using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.Sample.Redis.Implementation
{
    public class RepositoryBase
    {
        protected readonly ConnectionMultiplexer _connector;
        protected readonly string _providerNamespace;

        public RepositoryBase(ConnectionSettingsProvider settings)
        {
            _connector = settings.Multiplexer;
            _providerNamespace = settings.ProviderNamespace;
        }

        protected string GetKeyForDocument(Guid id)
        {
            return string.Format("{0}:document:{1:N}", _providerNamespace, id);
        }

        protected string GetKeyForEmployee(Guid id)
        {
            return string.Format("{0}:employee:{1:N}", _providerNamespace, id);
        }

        protected string GetKeyForStructDivision(Guid id)
        {
            return string.Format("{0}:structdivision:{1:N}", _providerNamespace, id);
        }

        /// <summary>
        /// Key for List of Document Ids
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        protected string GetKeyForInboxDocuments(Guid employeeId)
        {
            return GetKeyForInboxDocuments(employeeId.ToString("N"));
        }

        /// <summary>
        /// Key for List of Document Ids
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        protected string GetKeyForInboxDocuments(string employeeId)
        {
            //to avoid different GUID formats
            var employeeGuid = new Guid(employeeId);
            return string.Format("{0}:inbox:documents:{1:N}", _providerNamespace, employeeGuid);
        }

        /// <summary>
        /// Key for Set of Employee Ids
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        protected string GetKeyForInboxEmployees(Guid documentId)
        {
            return string.Format("{0}:inbox:employees:{1:N}", _providerNamespace, documentId);
        }

        /// <summary>
        /// Key for SortedSet of Document Ids
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        protected string GetKeyForOutboxDocuments(Guid employeeId)
        {
            return string.Format("{0}:outbox:documents:{1:N}", _providerNamespace, employeeId);
        }

        protected string GetKeyForSortedDocuments()
        {
            return string.Format("{0}:documents:sorted", _providerNamespace);
        }

        protected string GetkeyForDocumentNumber()
        {
            return string.Format("{0}:documents:number", _providerNamespace);
        }

        protected string GetkeyForOutboxNumber()
        {
            return string.Format("{0}:outbox:number", _providerNamespace);
        }

        /// <summary>
        /// Key for Set of Employees Ids
        /// </summary>
        /// <returns></returns>
        protected string GetKeyForEmployeesSet()
        {
            return string.Format("{0}:employees:set", _providerNamespace);
        }

        /// <summary>
        /// Key for Set of Role Names
        /// </summary>
        /// <returns></returns>
        protected string GetKeyForRolesSet()
        {
            return string.Format("{0}:roles:set", _providerNamespace);
        }

        /// <summary>
        /// Key for Set of StructDivisions Ids
        /// </summary>
        /// <returns></returns>
        protected string GetKeyForStructDivisionsSet()
        {
            return string.Format("{0}:structdivisions:set", _providerNamespace);
        }

        /// <summary>
        /// Key for Set of Employees Ids
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        protected string GetKeyForEmployeesInRole(string role)
        {
            return string.Format("{0}:roles:employees:{1}", _providerNamespace, role);
        }

        protected Entities.Employee GetEmployee(IDatabase database, Guid id)
        {
            var doc = database.StringGet(GetKeyForEmployee(id));

            if (doc.HasValue)
            {
                return JsonConvert.DeserializeObject<Entities.Employee>(doc);
            }

            return null;
        }
    }
}
