using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Budget2.Server.Security.Interface.DataContracts;

namespace Budget2.Server.Security.Interface.Services
{
    public interface IAuthenticationService
    {
        bool Authenticate(Guid securityToken);
        bool IsAuthenticated();
        ServiceIdentity GetCurrentIdentity();
    }
}
