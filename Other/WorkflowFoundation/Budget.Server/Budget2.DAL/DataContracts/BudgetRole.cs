using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.DAL.DataContracts
{
    public class BudgetRole
    {
        public Guid Id
        {
            get; private set;
        }

        public static readonly BudgetRole Controller = new BudgetRole()
                                                           {Id = new Guid("854805A8-6398-4CB0-9B82-77476AD3DECB")};

        public static readonly BudgetRole Curator = new BudgetRole() { Id = new Guid("B2B83773-FC0A-4541-9493-60B0BE914675") };

        public static readonly BudgetRole UPKZHead = new BudgetRole() { Id = new Guid("5376D41E-AA83-4D49-AA51-E9F97ED7A75A") };

        public static readonly BudgetRole DivisionHead = new BudgetRole() { Id = new Guid("B64D5C08-5763-40E1-A4A0-3486AB145E86") };

        public static readonly BudgetRole Expert = new BudgetRole() { Id = new Guid("E5C06C2D-5DA1-4A4A-961C-382B94898D39") };

        public static readonly BudgetRole Accountant = new BudgetRole()
                                                           {Id = new Guid("335AF96D-8E4F-4906-BA25-8D6722D15564")};

        public static readonly BudgetRole FullControl = new BudgetRole() { Id = Guid.Empty };
    }
}
