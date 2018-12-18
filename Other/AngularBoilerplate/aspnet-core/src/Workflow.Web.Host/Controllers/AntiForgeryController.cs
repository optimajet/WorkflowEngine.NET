using Microsoft.AspNetCore.Antiforgery;
using Workflow.Controllers;

namespace Workflow.Web.Host.Controllers
{
    public class AntiForgeryController : WorkflowControllerBase
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
