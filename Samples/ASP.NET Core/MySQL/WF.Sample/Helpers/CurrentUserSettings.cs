using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace WF.Sample.Helpers
{
    public class CurrentUserSettings
    {
        public static Guid GetCurrentUser(HttpContext context)
        {
            Guid res = Guid.Empty;
            if (context.Request.Query["CurrentEmployee"].FirstOrDefault() != null)
            {
                Guid.TryParse(context.Request.Query["CurrentEmployee"].FirstOrDefault(), out res);
                SetUserInCookies(context, res);
            }
            else if (context.Request.Cookies["CurrentEmployee"] != null)
            {
                Guid.TryParse(context.Request.Cookies["CurrentEmployee"], out res);
            }
            return res;
        }

        public static void SetUserInCookies(HttpContext context, Guid userId)
        {
            context.Response.Cookies.Append("CurrentEmployee", userId.ToString());
        }
    }
}