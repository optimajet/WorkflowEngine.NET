using System;
using System.Security.Principal;

namespace Budget2.Server.Security.Interface.DataContracts
{
    public class ServiceIdentity : IIdentity 
    {
        public Guid Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public Guid? ImpersonatedId
        {
            get;
            private set;
        }

        public string AuthenticationType
        {
            get { return string.Empty; }
        }

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public ServiceIdentity (string name, Guid id, bool isAuthenticated)
        {
            Name = name;
            Id = id;
            IsAuthenticated = isAuthenticated;
        }

        public bool TryImpersonate (Guid impersonatedId)
        {
            if (impersonatedId != Guid.Empty && impersonatedId != Id)
            {
                ImpersonatedId = impersonatedId;
                return true;
            }
            return false;
        }

        public bool IsImpersonated
        {
            get
            {
                return ImpersonatedId.HasValue;
            }
        }
    }
}
