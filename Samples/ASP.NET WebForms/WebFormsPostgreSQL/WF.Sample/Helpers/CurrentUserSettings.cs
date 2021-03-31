using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace WF.Sample.Helpers
{
    public class CurrentUserSettings
    {
        public static Guid GetCurrentUser()
        {
            Guid res = Guid.Empty;
            if (HttpContext.Current.Request.QueryString["CurrentEmployee"] != null)
            {
                Guid.TryParse(HttpContext.Current.Request.QueryString["CurrentEmployee"], out res);
                SetUserInCookies(res);
            }
            else if (HttpContext.Current.Request.Cookies["CurrentEmployee"] != null)
            {
                Guid.TryParse(HttpContext.Current.Request.Cookies["CurrentEmployee"].Value, out res);
            }
            return res;
        }

        public static void SetUserInCookies(Guid userId)
        {
            HttpContext.Current.Response.SetCookie(new HttpCookie("CurrentEmployee", userId.ToString()));
        }
    }
}
