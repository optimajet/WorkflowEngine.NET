using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using Budget2.DAL;
using Budget2.Server.Security.Interface.DataContracts;
using Budget2.Server.Security.Interface.Services;


namespace Budget2.Server.Security.Services
{
    public class AuthenticationService : Budget2DataContextService, IAuthenticationService
    {
        //TODO Временное некорректное решение подписывание строк ключами сделать
        public bool Authenticate(Guid securityToken)
        {
            using (var context = CreateContext())
            {
                var trustee = context.SecurityTrustees.SingleOrDefault(p => p.Id == securityToken);
                if (trustee != null)
                {
                    SetIdentity(trustee);
                    return true;
                }
            }

            return false;
        }

        public bool IsAuthenticated ()
        {
            if (HttpContext.Current.User == null)
                return false;

            else return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public ServiceIdentity GetCurrentIdentity ()
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null)
            {
                if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
                    return Thread.CurrentPrincipal.Identity as ServiceIdentity;
                return null;
            }

            return HttpContext.Current.User.Identity as ServiceIdentity;
        }

        private void SetIdentity (SecurityTrustee identity)
        {
            HttpContext.Current.User = new GenericPrincipal(new ServiceIdentity(identity.Name, identity.Id, true),
                                                            new string[] {});
        }
    }
}
