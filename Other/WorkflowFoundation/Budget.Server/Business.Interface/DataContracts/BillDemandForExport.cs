using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface
{
    public class BillDemandForExport
    {
        public IEnumerable<int> PaymentPlanItemIds { get; set; }
        public Guid Id { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime CurrencyRevisionDate { get; set; }
        public string Number { get; set; }
        public long IdNumber { get; set; }
        public long CustomerCounteragentId { get; set; }
        public long CounteragentId { get; set; }
        public decimal SumWIthNDS {get;set;}
        public decimal NDSTaxValue { get; set; }
        public string ContractId { get; set; }
        public DateTime DocumentDate { get; set; }
        public string FirmCode { get; set; }
        public DateTime? AccountDate { get; set; }
        public string OPCode { get; set; }
        public string ProjectCode { get; set; }
        public string PPCode { get; set; }
        public string SmetaCode { get; set; }
        public byte BudgetPartId { get; set; }
    }
}
