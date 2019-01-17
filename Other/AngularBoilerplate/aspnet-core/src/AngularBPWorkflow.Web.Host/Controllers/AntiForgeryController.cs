using Microsoft.AspNetCore.Antiforgery;
using AngularBPWorkflow.Controllers;

namespace AngularBPWorkflow.Web.Host.Controllers
{
    public class AntiForgeryController : AngularBPWorkflowControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
