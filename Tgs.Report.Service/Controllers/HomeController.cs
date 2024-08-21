using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.Report.Service.Controllers
{
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [Route("/")]
        [HttpGet]
        [Description("Home")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return Ok("Report API");
        }
    }
}
