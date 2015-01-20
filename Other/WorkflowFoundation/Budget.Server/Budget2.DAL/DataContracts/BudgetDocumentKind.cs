using System.Collections.Generic;

namespace Budget2.DAL.DataContracts
{
    public class BudgetDocumentKind
    {

        public byte Id { get; set; }

        public static readonly BudgetDocumentKind InBudget = new BudgetDocumentKind { Id = 0 };

        public static readonly BudgetDocumentKind NotInBudget = new BudgetDocumentKind { Id = 1 };

         public static readonly IEnumerable<BudgetDocumentKind> All = new List<BudgetDocumentKind> { InBudget, NotInBudget};  
    }
}
