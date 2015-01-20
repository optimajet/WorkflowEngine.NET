using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Budget2.Server.Business.Interface.DataContracts
{
    public class Employee : IEquatable<Employee>
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid IdentityId { get; set; }
        public bool IsSendNotification { get; set; }


        public bool Equals(Employee other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
